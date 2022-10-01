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
    public static List<ExtendedChannel> AnonJoinedChannels { get; } = new List<ExtendedChannel>();
    public static string[] JLChannels { get; private set; } = Array.Empty<string>();
    public static List<ExtendedChannel> FetchedChannels { get; private set; } = DbQueries.NewInstance().GetChannels().Result.ToList();

    private static readonly List<ExtendedChannel> JoinFailureChannels = new();
    private static bool IsInProgress { get; set; } = false;
    #endregion

    #region Initialization
    internal static async Task Connect(bool isReconnect)
    {
        if (IsInProgress) return;
        IsInProgress = true;

        if (isReconnect)
        {
            MainJoinedChannels.Clear();
            MainJoinedChannelNames.Clear();
            AnonJoinedChannels.Clear();
            StreamMonitor.Stop();
        }

        Log.Information($"Starting to {(isReconnect ? "re" : string.Empty)}join channels");
        RegisterEvents(isReconnect);
        JLChannels = (await ExternalAPIHandler.GetIvrChannels()).Channels.Select(x => x.Name).ToArray();

        await "twitch:channels".SetKey(FetchedChannels);

        IAsyncEnumerable<ExtendedChannel> c = new AsyncEnumerable<ExtendedChannel>(async y =>
        {
            for (int i = 0; i < FetchedChannels.Count; i++) await y.ReturnAsync(FetchedChannels[i]);
            y.Break();
        });

        await c.ForEachAsync(async x =>
        {
            // Assume the channel is joined until being told otherwise
            if (x.Priority >= 50)
            {
                MainClient.Client.JoinChannel(x.Username);
                Log.Debug($"[Main] Queued join: {x.Username} (JustLog:{JLChannels.Contains(x.Username)})");
                await Task.Delay(300);
            }
            Log.Debug($"[Anon] Queued join: {x.Username}");
            await Task.Delay(300);
        });
        c = default!;
        IsInProgress = false;
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

        try
        {
            ExtendedChannel target = AnonJoinedChannels.First(x => x.Username == channel);
            _ = AnonJoinedChannels.Remove(target);

            var db = new DbQueries();
            _ = await db.RemoveChannel(target);
        }
        catch (Exception)
        {
            Log.Error($"AnonymousClient failed to part channel \"{channel}\"");
        }

        try
        {
            ExtendedChannel target = MainJoinedChannels.First(x => x.Username == channel);
            _ = MainJoinedChannels.Remove(target);
            _ = MainJoinedChannelNames.Remove(channel);
            MainClient.Client.LeaveChannel(channel);
        }
        catch (Exception)
        {
            Log.Error($"MainClient failed to part channel \"{channel}\"");
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
            MessageHandler.SendColoredMessage(Config.RelayChannel, $"Channel size changed: {pCount} -> {cCount}", ChatColor.YellowGreen);
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

    private static void AnonOnLeftChannel(object? sender, OnLeftChannelArgs e)
    {
        Log.Information($"[Anon] Left channel {e.Channel}");
        _ = AnonJoinedChannels.Remove(FetchedChannels.First(x => x.Username == e.Channel));
    }

    private static void AnonOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        Log.Information($"[Anon] Joined channel {e.Channel}");
        AnonJoinedChannels.Add(FetchedChannels.First(x => x.Username == e.Channel));
    }

    private static void MainOnFailedJoin(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
    {
        Log.Warning($"[Main] Failed to join {e.Exception.Channel}: {e.Exception.Details}");
        JoinFailureChannels.Add(FetchedChannels.First(x => x.Username == e.Exception.Channel));
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

    private static readonly LiveStreamMonitorService MonitoringService = new(TwitchAPIHandler.API, 30);
    #endregion

    #region Controls
    public static void Start()
    {
        List<ExtendedChannel> Channels = ChannelHandler.FetchedChannels;
        StreamData = Channels.ToDictionary(
            x => x.Username,
            y => new Stream(y.Username, false, string.Empty, string.Empty, DateTime.Now));
        MonitoringService.SetChannelsByName(Channels.Where(x => x.Priority >= 0).Select(x => x.Username).ToList());

        MonitoringService.OnServiceStarted += ServiceStarted;
        MonitoringService.OnStreamOnline += StreamOnline;
        MonitoringService.OnStreamUpdate += StreamUpdate;
        MonitoringService.OnStreamOffline += StreamOffline;
        MonitoringService.Start();

        Time.DoEvery(TimeSpan.FromHours(6), () => Reset());
    }

    public static void Reset()
    {
        StreamData.Clear();
        List<ExtendedChannel> Channels = ChannelHandler.FetchedChannels;
        MonitoringService.SetChannelsByName(Channels.Select(x => x.Username).ToList());
    }

    public static void Stop()
    {
        MonitoringService.OnServiceStarted -= ServiceStarted;
        MonitoringService.OnStreamOnline -= StreamOnline;
        MonitoringService.OnStreamUpdate -= StreamUpdate;
        MonitoringService.OnStreamOffline -= StreamOffline;
        MonitoringService.Stop();
    }
    #endregion

    #region Monitor events
    private static void StreamOffline(object? sender, OnStreamOfflineArgs e)
    {
        TimeSpan uptime = DateTime.Now - StreamData[e.Channel].Started.ToLocalTime();
        StreamData[e.Channel] = new Stream(e.Channel, false, e.Stream.Title, e.Stream.GameName, DateTime.Now);
        MessageHandler.SendColoredMessage(
            Config.RelayChannel,
            $"{RandomReplies.StreamOfflineEmotes.Choice()} @{e.Channel} is now offline! -- {uptime.FormatTimeLeft()}",
            ChatColor.GoldenRod);
    }

    private static void StreamUpdate(object? sender, OnStreamUpdateArgs e)
    {
        var current = new Stream(e.Channel, false, e.Stream.Title, e.Stream.GameName, e.Stream.StartedAt);
        if (StreamData[e.Channel].Title != e.Stream.Title
        || StreamData[e.Channel].GameName != e.Stream.GameName)
        {
            TimeSpan uptime = DateTime.Now - StreamData[e.Channel].Started.ToLocalTime();
            StreamData[e.Channel] = current;
            MessageHandler.SendColoredMessage(
                Config.RelayChannel,
                $"{RandomReplies.StreamUpdateEmotes.Choice()} @{e.Channel} updated their stream: {e.Stream.Title} -- {e.Stream.GameName} -- {uptime.FormatTimeLeft()}",
                ChatColor.DodgerBlue);
        }
    }

    private static void StreamOnline(object? sender, OnStreamOnlineArgs e)
    {
        var current = new Stream(e.Channel, false, e.Stream.Title, e.Stream.GameName, e.Stream.StartedAt);
        StreamData[e.Channel] = current;
        MessageHandler.SendColoredMessage(
            Config.RelayChannel,
            $"{RandomReplies.StreamOnlineEmotes.Choice()} @{e.Channel} has gone live: {e.Stream.Title} - {e.Stream.GameName}",
            ChatColor.SpringGreen);
    }

    private static void ServiceStarted(object? sender, OnServiceStartedArgs e)
    {
        MessageHandler.SendMessage(Config.RelayChannel, $"OBSOLETE Hello");
    }
    #endregion

    internal sealed record Stream(string Username, bool IsOnline, string Title, string GameName, DateTime Started);
}
