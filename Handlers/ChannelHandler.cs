using Dasync.Collections;
using Tack.Core;
using Tack.Database;
using Tack.Misc;
using Tack.Models;
using Tack.Utils;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Events;

namespace Tack.Handlers;
internal static class ChannelHandler
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
        if (_isInProgress) return;
        _isInProgress = true;

        if (isReconnect)
        {
            MainJoinedChannels.Clear();
            MainJoinedChannelNames.Clear();
            StreamMonitor.Stop();
        }

        Log.Information($"Starting to {(isReconnect ? "re" : string.Empty)}join channels");
        RegisterEvents(isReconnect);

        await "twitch:channels".SetKey(FetchedChannels);

        IAsyncEnumerable<ExtendedChannel> c = new AsyncEnumerable<ExtendedChannel>(async y =>
        {
            for (int i = 0; i < FetchedChannels.Count; i++) await y.ReturnAsync(FetchedChannels[i]);
            y.Break();
        });

        await c.ForEachAsync(async x =>
        {
            if (x.Priority >= 50)
            {
                MainClient.Client.JoinChannel(x.Username);
                Log.Debug($"[Main] Queued join: {x.Username}");
                await Task.Delay(1000);
            }
        });
        c = default!;
        _isInProgress = false;
        StreamMonitor.Start();
        Time.DoEvery(TimeSpan.FromHours(1), async () =>
        {
            await ReloadFetchedChannels();
            await "twitch:channels".SetKey(FetchedChannels);
        });
    }
    #endregion

    #region Methods
    /// <returns>True if successful; Otherwise false</returns>
    public static async Task<bool> JoinChannel(string channel, int priority = 0, bool logged = true)
    {
        if (FetchedChannels.Any(x => x.Username == channel)) return false;
        var uf = new UserFactory();
        var c = new Channel(channel, priority, logged);
        ExtendedChannel? ec = await uf.CreateChannelProfile(c);
        if (ec is null) return false;
        FetchedChannels.Add(ec);

        if (priority >= 50) MainClient.Client.JoinChannel(channel);

        var db = new DbQueries();
        bool s = await db.AddChannel(ec);
        return s;
    }

    /// <returns>True if successful; Otherwise false</returns>
    public static async Task<bool> PartChannel(string channel)
    {
        bool fetched = FetchedChannels.Any(x => x.Username == channel);

        ExtendedChannel? target = FetchedChannels.FirstOrDefault(x => x.Username == channel);
        if (target is null) return false;

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
            Log.Error(ex, $"Erros occured whilst trying to part {channel} :");
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
            MessageHandler.SendColoredMessage(AppConfigLoader.Config.RelayChannel, $"Channel size changed: {pCount} -> {cCount}", ChatColor.YellowGreen);
        }
    }

    private static void RegisterEvents(bool isReconnect)
    {
        if (isReconnect) return;

        MainClient.Client.OnJoinedChannel += MainOnJoinedChannel;
        MainClient.Client.OnLeftChannel += MainOnLeftChannel;
        MainClient.Client.OnFailureToReceiveJoinConfirmation += MainOnFailedJoin;
    }
    #endregion

    #region Client events
    private static void AnonOnFailedJoin(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
    {
        Log.Warning($"[Anon] Failed to join {e.Exception.Channel}: {e.Exception.Details}");
    }

    private static void MainOnFailedJoin(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
    {
        Log.Warning($"[Main] Failed to join {e.Exception.Channel}: {e.Exception.Details}");
        _joinFailureChannels.Add(FetchedChannels.First(x => x.Username == e.Exception.Channel));
        _ = MainJoinedChannelNames.Remove(e.Exception.Channel);
    }

    private static void MainOnLeftChannel(object? sender, OnLeftChannelArgs e)
    {
        Log.Information($"[Main] Left channel {e.Channel}");
        _ = MainJoinedChannels.Remove(FetchedChannels.First(x => x.Username == e.Channel));
        _ = MainJoinedChannelNames.Remove(e.Channel);
    }

    private static void MainOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        Log.Information($"[Main] Joined channel {e.Channel}");
        MainJoinedChannels.Add(FetchedChannels.First(x => x.Username == e.Channel));
        MainJoinedChannelNames.Add(e.Channel);
    }
    #endregion

    public sealed record Channel(string Name, int Priority, bool Logged);
}

internal static class StreamMonitor
{
    #region Properties
    public static Dictionary<string, Stream> StreamData { get; private set; } = new Dictionary<string, Stream>();

    private static readonly LiveStreamMonitorService _monitoringService = new(TwitchAPIHandler.API, 30);
    private static readonly string _relayChannel = AppConfigLoader.Config.RelayChannel;
    #endregion

    #region Controls
    public static void Start()
    {
        List<ExtendedChannel> Channels = ChannelHandler.FetchedChannels;
        StreamData = Channels.ToDictionary(
            x => x.Username,
            y => new Stream(y.Username, false, string.Empty, string.Empty, DateTime.Now));
        _monitoringService.SetChannelsByName(Channels.Where(x => x.Priority >= 0).Select(x => x.Username).ToList());

        _monitoringService.OnServiceStarted += ServiceStarted;
        _monitoringService.OnStreamOnline += StreamOnline;
        _monitoringService.OnStreamUpdate += StreamUpdate;
        _monitoringService.OnStreamOffline += StreamOffline;
        _monitoringService.Start();

        Time.DoEvery(TimeSpan.FromHours(6), () => Reset());
        Time.DoEvery(300, async () => await "twitch:channels:streams".SetKey(StreamData.Select(x => x.Value)));
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
    private static void StreamOffline(object? sender, OnStreamOfflineArgs e)
    {
        TimeSpan uptime = Time.Since(StreamData[e.Channel].Started);
        StreamData[e.Channel] = new Stream(e.Channel, false, e.Stream.Title, e.Stream.GameName, DateTime.Now);
        MessageHandler.SendColoredMessage(
            _relayChannel,
            $"{RandomReplies.StreamOfflineEmotes.Choice()} @{e.Channel} is now offline! -- {uptime.FormatTimeLeft()}",
            ChatColor.GoldenRod);
    }

    private static void StreamUpdate(object? sender, OnStreamUpdateArgs e)
    {
        if (StreamData[e.Channel].Title != e.Stream.Title
        || StreamData[e.Channel].GameName != e.Stream.GameName)
        {
            TimeSpan uptime = Time.Since(StreamData[e.Channel].Started);
            StreamData[e.Channel] = new Stream(e.Channel, true, e.Stream.Title, e.Stream.GameName, e.Stream.StartedAt);
            MessageHandler.SendColoredMessage(
                _relayChannel,
                $"{RandomReplies.StreamUpdateEmotes.Choice()} @{e.Channel} updated their stream: {e.Stream.Title} -- {e.Stream.GameName} -- {uptime.FormatTimeLeft()}",
                ChatColor.DodgerBlue);
        }
    }

    private static void StreamOnline(object? sender, OnStreamOnlineArgs e)
    {
        StreamData[e.Channel] = new Stream(e.Channel, true, e.Stream.Title, e.Stream.GameName, e.Stream.StartedAt);
        MessageHandler.SendColoredMessage(
            _relayChannel,
            $"{RandomReplies.StreamOnlineEmotes.Choice()} @{e.Channel} has gone live: {e.Stream.Title} - {e.Stream.GameName}",
            ChatColor.SpringGreen);
    }

    private static void ServiceStarted(object? sender, OnServiceStartedArgs e)
    {
        MessageHandler.SendMessage(_relayChannel, $"OBSOLETE Hello");
    }
    #endregion

    internal sealed record Stream(string Username, bool IsOnline, string Title, string GameName, DateTime Started);
}
