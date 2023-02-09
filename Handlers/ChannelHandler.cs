using Dasync.Collections;
using Tack.Core;
using Tack.Database;
using Tack.Misc;
using Tack.Models;
using Tack.Utils;
using TwitchLib.Api.Helix.Models.Chat;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;
using TwitchLibStream = TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream;

namespace Tack.Handlers;
public static class ChannelHandler
{
    #region Properties
    public static List<ExtendedChannel> MainJoinedChannels { get; } = new List<ExtendedChannel>();
    public static List<string> MainJoinedChannelNames { get; } = new List<string>();
    public static List<ExtendedChannel> FetchedChannels { get; private set; } = DbQueries.NewInstance().GetChannels().Result.ToList();

    private static readonly List<ExtendedChannel> _joinFailureChannels = new();
    private static bool _isInProgress = false;
    #endregion

    #region Initialization
    internal static async Task Connect(bool isReconnect)
    {
        if (_isInProgress)
            return;
        _isInProgress = true;

        if (isReconnect)
        {
            MainJoinedChannels.Clear();
            MainJoinedChannelNames.Clear();
            StreamMonitor.Stop();
        }

        RegisterEvents(isReconnect);

        await Redis.Cache.SetObjectAsync("twitch:channels", FetchedChannels);

        IAsyncEnumerable<ExtendedChannel> c = new AsyncEnumerable<ExtendedChannel>(async y =>
        {
            for (int i = 0; i < FetchedChannels.Count; i++)
                await y.ReturnAsync(FetchedChannels[i]);
            y.Break();
        });

        await c.ForEachAsync(async x =>
        {
            if (x.Priority >= 50)
            {
                MainClient.Client.JoinChannel(x.Username);
                Log.Debug("[Main] Queued join: {username}", x.Username);
                await Task.Delay(1000);
            }
        });
        c = default!;
        _isInProgress = false;
        StreamMonitor.Start();
        Time.DoEvery(TimeSpan.FromHours(1), async () =>
        {
            await ReloadFetchedChannels();
            await Redis.Cache.SetObjectAsync("twitch:channels", FetchedChannels);
        });
    }
    #endregion

    #region Methods
    /// <returns>True if successful; Otherwise false</returns>
    public static async Task<bool> JoinChannel(string channel, int priority = 0, bool logged = true)
    {
        if (FetchedChannels.Any(x => x.Username == channel))
            return false;
        var c = new Channel(channel, priority, logged);
        var extendedChannel = await User.GetChannel(c);
        if (!extendedChannel.Success)
            return false;
        FetchedChannels.Add(extendedChannel.Value);

        if (priority >= 50)
            MainClient.Client.JoinChannel(channel);

        var db = new DbQueries();
        bool s = await db.AddChannel(extendedChannel.Value);
        return s;
    }

    /// <returns>True if successful; Otherwise false</returns>
    public static async Task<bool> PartChannel(string channel)
    {
        bool fetched = FetchedChannels.Any(x => x.Username == channel);

        ExtendedChannel? target = FetchedChannels.FirstOrDefault(x => x.Username == channel);
        if (target is null)
            return false;

        try
        {
            var db = new DbQueries();
            _ = await db.RemoveChannel(target);
            _ = MainJoinedChannels.Remove(target);
            _ = MainJoinedChannelNames.Remove(channel);
            MainClient.Client.LeaveChannel(channel);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Errors occured whilst trying to part {channel} :", channel);
        }

        return fetched;
    }

    public static async Task ReloadFetchedChannels()
    {
        int pCount = FetchedChannels.Count;
        using (var db = new DbQueries())
        {
            FetchedChannels = (await db.GetChannels()).ToList();
        }
        int cCount = FetchedChannels.Count;

        if (pCount != cCount)
        {
            await MessageHandler.SendColoredMessage(AppConfigLoader.Config.RelayChannel, $"Channel size changed: {pCount} -> {cCount}", UserColors.YellowGreen);
        }
    }

    private static void RegisterEvents(bool isReconnect)
    {
        if (isReconnect)
            return;

        MainClient.Client.OnJoinedChannel += MainOnJoinedChannel;
        MainClient.Client.OnLeftChannel += MainOnLeftChannel;
        MainClient.Client.OnFailureToReceiveJoinConfirmation += MainOnFailedJoin;
    }
    #endregion

    #region Client events

    private static void MainOnFailedJoin(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
    {
        Log.Warning("[Main] Failed to join {channel}: {details}",
            e.Exception.Channel,
            e.Exception.Details);
        _joinFailureChannels.Add(FetchedChannels.First(x => x.Username == e.Exception.Channel));
        _ = MainJoinedChannelNames.Remove(e.Exception.Channel);
    }

    private static void MainOnLeftChannel(object? sender, OnLeftChannelArgs e)
    {
        Log.Information("[Main] Left channel {channel}",
            e.Channel);
        _ = MainJoinedChannels.Remove(FetchedChannels.First(x => x.Username == e.Channel));
        _ = MainJoinedChannelNames.Remove(e.Channel);
    }

    private static void MainOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        Log.Information("[Main] Joined channel {channel}",
            e.Channel);
        MainJoinedChannels.Add(FetchedChannels.First(x => x.Username == e.Channel));
        MainJoinedChannelNames.Add(e.Channel);
    }
    #endregion

    public sealed record Channel(string Name, int Priority, bool Logged);
}

internal static class StreamMonitor
{
    #region Properties
    public static Dictionary<string, TwitchStream> StreamData { get; private set; } = new();

    private static readonly LiveStreamMonitorService _monitoringService = new(TwitchAPIHandler.Instance.Api, 60);
    private static readonly string _relayChannel = AppConfigLoader.Config.RelayChannel;
    #endregion

    #region Controls
    public static void Start()
    {
        List<ExtendedChannel> Channels = ChannelHandler.FetchedChannels;
        StreamData = Channels.ToDictionary(
            x => x.Username,
            y => new TwitchStream(y.Username, false, string.Empty, string.Empty, DateTime.Now));
        _monitoringService.SetChannelsByName(Channels.Where(x => x.Priority >= 0).Select(x => x.Username).ToList());

        _monitoringService.OnServiceStarted += ServiceStarted;
        _monitoringService.OnStreamOnline += StreamOnline;
        _monitoringService.OnStreamUpdate += StreamUpdate;
        _monitoringService.OnStreamOffline += StreamOffline;
        _monitoringService.Start();

        Time.DoEvery(TimeSpan.FromHours(6), () => Reset());
        Time.DoEvery(TimeSpan.FromMinutes(5), async () => await Redis.Cache.SetObjectAsync("twitch:channels:streams", StreamData.Select(x => x.Value)));
    }

    public static void Reset()
    {
        StreamData.Clear();
        List<ExtendedChannel> Channels = ChannelHandler.FetchedChannels;
        _monitoringService.SetChannelsByName(Channels.Where(x => x.Priority >= 0).Select(x => x.Username).ToList());
    }

    public static void Stop()
    {
        _monitoringService.OnServiceStarted -= ServiceStarted;
        _monitoringService.OnStreamOnline -= StreamOnline;
        _monitoringService.OnStreamUpdate -= StreamUpdate;
        _monitoringService.OnStreamOffline -= StreamOffline;
        _monitoringService.Stop();
    }
    #endregion

    #region Monitor events
    private static async void StreamOffline(object? sender, OnStreamOfflineArgs e)
    {
        Log.Information("[{header}] {channel} has gone offline!", nameof(StreamMonitor), e.Channel);
        TimeSpan uptime = Time.Since(StreamData[e.Channel].Started);
        UpdateDict(e.Channel, e.Stream, nameof(StreamOffline));

        await MessageHandler.SendColoredMessage(
            _relayChannel,
            $"{RandomReplies.StreamOfflineEmotes.Choice()} @{e.Channel} is now offline! -- {uptime.FormatTimeLeft()}",
            UserColors.GoldenRod);
    }

    private static async void StreamUpdate(object? sender, OnStreamUpdateArgs e)
    {
        Log.Debug("[{header}] {channel} tick", nameof(StreamMonitor), e.Channel);
        if (StreamData[e.Channel].Title != e.Stream.Title
        || StreamData[e.Channel].GameName != e.Stream.GameName)
        {
            TimeSpan uptime = Time.Since(StreamData[e.Channel].Started);
            UpdateDict(e.Channel, e.Stream, nameof(StreamUpdate));

            await MessageHandler.SendColoredMessage(
                _relayChannel,
                $"{RandomReplies.StreamUpdateEmotes.Choice()} @{e.Channel} updated their stream: {e.Stream.Title} -- {e.Stream.GameName} -- {uptime.FormatTimeLeft()}",
                UserColors.DodgerBlue);
        }
    }

    private static async void StreamOnline(object? sender, OnStreamOnlineArgs e)
    {
        Log.Information("[{header}] {channel} has gone live!", nameof(StreamMonitor), e.Channel);
        UpdateDict(e.Channel, e.Stream, nameof(StreamOnline));

        await MessageHandler.SendColoredMessage(
            _relayChannel,
            $"{RandomReplies.StreamOnlineEmotes.Choice()} @{e.Channel} has gone live: {e.Stream.Title} - {e.Stream.GameName}",
            UserColors.SpringGreen);
    }

    private static void UpdateDict(string channel, TwitchLibStream stream, string type)
    {
        switch (type)
        {
            case nameof(StreamOnline):
                if (!StreamData.ContainsKey(channel))
                    StreamData.Add(channel, new(stream.UserName, true, stream.Title, stream.GameName, DateTime.Now));
                else
                {
                    StreamData[channel].IsOnline = true;
                    StreamData[channel].Title = stream.Title;
                    StreamData[channel].GameName = stream.GameName;
                    StreamData[channel].Started = DateTime.Now;
                }
                break;

            case nameof(StreamOffline):
                StreamData[channel].IsOnline = false;
                break;

            case nameof(StreamUpdate):
                StreamData[channel].Title = stream.Title;
                StreamData[channel].GameName = stream.GameName;
                break;
        }
    }

    private static void ServiceStarted(object? sender, OnServiceStartedArgs e)
    {
#if !DEBUG
        MessageHandler.SendMessage(_relayChannel, $"OBSOLETE Hello");
#endif
    }
    #endregion

    internal sealed class TwitchStream
    {
        public string Username { get; init; }
        public bool IsOnline { get; set; }
        public string Title { get; set; }
        public string GameName { get; set; }
        public DateTime Started { get; set; }

        public TwitchStream(string username, bool isOnline, string title, string gameName, DateTime started)
        {
            Username = username;
            IsOnline = isOnline;
            Title = title;
            GameName = gameName;
            Started = started;
        }
    }
}
