using MiniTwitch.Irc.Interfaces;
using MiniTwitch.Irc.Models;
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
    public static List<ExtendedChannel> FetchedChannels { get; private set; } = SingleOf<DbQueries>.Obj.GetChannels().Result.ToList();

    private static readonly AnonymousClient _anon = SingleOf<AnonymousClient>.Obj;
    private static readonly MainClient _main = SingleOf<MainClient>.Obj;
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
        foreach (ExtendedChannel channel in FetchedChannels)
        {
            await Task.Delay(600);
            if (channel.Priority >= 50)
            {
                if (!await _main.Client.JoinChannel(channel.Username))
                    Log.Warning("[{h}] Failed to join {c}", nameof(ChannelHandler), channel.Username);

                continue;
            }

            if (!await _anon.Client.JoinChannel(channel.Username))
                Log.Warning("[{h}] Failed to join {c}", nameof(ChannelHandler), channel.Username);
        }

        Log.Information("[{h}] {t} Joined: {c}", nameof(ChannelHandler), nameof(AnonymousClient), _anon.Client.JoinedChannels.Select(x => x.Name));
        Log.Information("[{h}] {t} Joined: {c}", nameof(ChannelHandler), nameof(MainClient), _main.Client.JoinedChannels.Select(x => x.Name));

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
        Result<ExtendedChannel> extendedChannel = await User.GetChannel(c);
        if (!extendedChannel.Success)
            return false;
        FetchedChannels.Add(extendedChannel.Value);

        if (priority >= 50)
        {
            if (await _main.Client.JoinChannel(channel))
                Log.Information("[{h}] Joined {c}", nameof(ChannelHandler), channel);
            else
                Log.Warning("[{h}] Failed to join {c}", nameof(ChannelHandler), channel);
        }
        else
        {
            if (await _anon.Client.JoinChannel(channel))
                Log.Information("[{h}] Joined {c}", nameof(ChannelHandler), channel);
            else
                Log.Warning("[{h}] Failed to join {c}", nameof(ChannelHandler), channel);
        }

        DbQueries db = SingleOf<DbQueries>.Obj;
        bool s = await db.AddChannel(extendedChannel.Value);
        return s;
    }

    /// <returns>True if successful; Otherwise false</returns>
    public static async Task<bool> PartChannel(string channel)
    {
        ExtendedChannel? fetched = FetchedChannels.FirstOrDefault(x => x.Username == channel);
        if (fetched is null)
            return false;

        try
        {
            DbQueries db = SingleOf<DbQueries>.Obj;
            if (fetched.Priority >= 50)
            {

                _ = MainJoinedChannels.Remove(fetched);
                _ = MainJoinedChannelNames.Remove(channel);
                await _main.Client.PartChannel(channel);
            }
            else
            {
                _ = _anon.Client.PartChannel(channel);
            }

            _ = await db.RemoveChannel(fetched);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Errors occured whilst trying to part {channel} :", channel);
        }

        return true;
    }

    public static async Task ReloadFetchedChannels()
    {
        int pCount = FetchedChannels.Count;
        FetchedChannels = (await SingleOf<DbQueries>.Obj.GetChannels()).ToList();

        int cCount = FetchedChannels.Count;

        if (pCount != cCount)
            await MessageHandler.SendColoredMessage(AppConfigLoader.Config.RelayChannel, $"Channel size changed: {pCount} -> {cCount}", UserColors.YellowGreen);
    }

    private static void RegisterEvents(bool isReconnect)
    {
        if (isReconnect)
            return;

        _main.Client.OnChannelJoin += MainOnJoinedChannel;
        _main.Client.OnChannelPart += MainOnLeftChannel;
    }
    #endregion

    #region Client events
    private static ValueTask MainOnLeftChannel(IPartedChannel channel)
    {
        _ = MainJoinedChannels.Remove(FetchedChannels.First(x => x.Username == channel.Name));
        _ = MainJoinedChannelNames.Remove(channel.Name);
        return ValueTask.CompletedTask;
    }

    private static ValueTask MainOnJoinedChannel(IrcChannel channel)
    {
        MainJoinedChannels.Add(FetchedChannels.First(x => x.Username == channel.Name));
        MainJoinedChannelNames.Add(channel.Name);
        return ValueTask.CompletedTask;
    }
    #endregion

    public sealed record Channel(string Name, int Priority, bool Logged);
}

internal static class StreamMonitor
{
    #region Properties
    public static Dictionary<string, TwitchStream> StreamData { get; private set; } = new();

    private static readonly LiveStreamMonitorService _monitoringService = new(TwitchApiHandler.Instance.Api, 60);
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
        if (!StreamData.ContainsKey(e.Channel))
        {
            StreamData.Add(e.Channel, new(e.Stream.UserName, true, e.Stream.Title, e.Stream.GameName, e.Stream.StartedAt));
        }

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
                {
                    StreamData.Add(channel, new(stream.UserName, true, stream.Title, stream.GameName, stream.StartedAt));
                }
                else
                {
                    StreamData[channel].IsOnline = true;
                    StreamData[channel].Title = stream.Title;
                    StreamData[channel].GameName = stream.GameName;
                    StreamData[channel].Started = stream.StartedAt;
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
