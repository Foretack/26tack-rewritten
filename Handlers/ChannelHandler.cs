using Dasync.Collections;
using Serilog;
using Tack.Core;
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
    private static readonly Database.Database Db = new Database.Database();
    #region Properties
    public static List<Channel> MainJoinedChannels { get; } = new List<Channel>();
    public static List<string> MainJoinedChannelNames { get; } = new List<string>();
    public static List<Channel> AnonJoinedChannels { get; } = new List<Channel>();
    public static string[] JLChannels { get; private set; } = Array.Empty<string>();
    public static List<Channel> FetchedChannels { get; private set; } = new List<Channel>(Db.GetChannels().Result);

    private static readonly List<Channel> JoinFailureChannels = new List<Channel>();
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
        JLChannels = (await ExternalAPIHandler.GetIvrChannels()).channels.Select(x => x.name).ToArray();
        IAsyncEnumerable<Channel> c = new AsyncEnumerable<Channel>(async y =>
        {
            for (int i = 0; i < FetchedChannels.Count; i++) await y.ReturnAsync(FetchedChannels[i]);
            y.Break();
        });
        await c.ForEachAsync(async x =>
        {
            // Assume the channel is joined until being told otherwise
            if (x.Priority >= 50)
            {
                MainClient.Client.JoinChannel(x.Name);
                Log.Debug($"[Main] Attempting to join: {x.Name} (JustLog:{JLChannels.Contains(x.Name)})");
                await Task.Delay(300);
            }
            AnonymousClient.Client.JoinChannel(x.Name);
            Log.Debug($"[Anon] Attempting to join: {x.Name}");
            await Task.Delay(300);
        });
        IsInProgress = false;
        StreamMonitor.Start();
    }
    #endregion

    public static async Task<bool> JoinChannel(string channel, int priority = 0, bool logged = true)
    {
        UserFactory uf = new UserFactory();
        Channel c = new Channel(channel, priority, logged);
        ExtendedChannel? ec = await uf.CreateChannelProfile(c);
        if (ec is null) return false;
        FetchedChannels.Add(c);

        if (priority >= 50) MainClient.Client.JoinChannel(channel);
        AnonymousClient.Client.JoinChannel(channel);

        Database.Database db = new Database.Database();
        bool s = await db.AddChannel(ec);
        return s;
    }

    public static async Task<bool> PartChannel(string channel)
    {
        try
        {
            Channel target = AnonJoinedChannels.First(x => x.Name == channel);
            AnonJoinedChannels.Remove(target);
            AnonymousClient.Client.LeaveChannel(channel);

            Database.Database db = new Database.Database();
            await db.RemoveChannel(target);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"AnonymousClient failed to part channel \"{channel}\"");
            return false;
        }

        try
        {
            Channel target = MainJoinedChannels.First(x => x.Name == channel);
            MainJoinedChannels.Remove(target);
            MainJoinedChannelNames.Remove(channel);
            MainClient.Client.LeaveChannel(channel);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"MainClient failed to part channel \"{channel}\"");
        }

        return true;
    }

    public static async Task ReloadFetchedChannels()
    {
        int pCount = FetchedChannels.Count;
        FetchedChannels = new List<Channel>(await Db.GetChannels());
        int cCount = FetchedChannels.Count;

        if (pCount != cCount)
        {
            MessageHandler.SendColoredMessage(Config.RelayChannel, $"❕ Channel size changed: {pCount} -> {cCount}", ChatColor.YellowGreen);
        }
    }

    private static void RegisterEvents(bool isReconnect)
    {
        if (isReconnect) return;

        MainClient.Client.OnJoinedChannel += MainOnJoinedChannel;
        MainClient.Client.OnLeftChannel += MainOnLeftChannel;
        MainClient.Client.OnFailureToReceiveJoinConfirmation += MainOnFailedJoin;

        AnonymousClient.Client.OnJoinedChannel += AnonOnJoinedChannel;
        AnonymousClient.Client.OnLeftChannel += AnonOnLeftChannel;
        AnonymousClient.Client.OnFailureToReceiveJoinConfirmation += AnonOnFailedJoin;
    }
    
    #region Client events
    private static void AnonOnFailedJoin(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
    {
        Log.Warning($"[Anon] Failed to join {e.Exception.Channel}: {e.Exception.Details}");
    }

    private static void AnonOnLeftChannel(object? sender, OnLeftChannelArgs e)
    {
        Log.Information($"[Anon] Left channel {e.Channel}");
        AnonJoinedChannels.Remove(FetchedChannels.First(x => x.Name == e.Channel));
    }

    private static void AnonOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        Log.Information($"[Anon] Joined channel {e.Channel}");
        AnonJoinedChannels.Add(FetchedChannels.First(x => x.Name == e.Channel));
    }

    private static void MainOnFailedJoin(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
    {
        Log.Warning($"[Main] Failed to join {e.Exception.Channel}: {e.Exception.Details}");
        JoinFailureChannels.Add(FetchedChannels.First(x => x.Name == e.Exception.Channel));
        MainJoinedChannelNames.Remove(e.Exception.Channel);
    }

    private static void MainOnLeftChannel(object? sender, OnLeftChannelArgs e)
    {
        Log.Information($"[Main] Left channel {e.Channel}");
        MainJoinedChannels.Remove(FetchedChannels.First(x => x.Name == e.Channel));
        MainJoinedChannelNames.Remove(e.Channel);
    }

    private static void MainOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        Log.Information($"[Main] Joined channel {e.Channel}");
        MainJoinedChannels.Add(FetchedChannels.First(x => x.Name == e.Channel));
        MainJoinedChannelNames.Add(e.Channel);
    }
    #endregion

    public record Channel(string Name, int Priority, bool Logged);
}

internal static class StreamMonitor
{
    #region Properties
    public static Dictionary<string, Stream> StreamData { get; private set; } = new Dictionary<string, Stream>();

    private static readonly LiveStreamMonitorService MonitoringService = new LiveStreamMonitorService(TwitchAPIHandler.API, 30);
    #endregion

    #region Controls
    public static void Start()
    {
        List<ChannelHandler.Channel> Channels = ChannelHandler.FetchedChannels;
        StreamData = Channels.ToDictionary(
            x => x.Name,
            y => new Stream(y.Name, false, string.Empty, string.Empty, DateTime.Now));
        MonitoringService.SetChannelsByName(Channels.Select(x => x.Name).ToList());

        MonitoringService.OnServiceStarted += ServiceStarted;
        MonitoringService.OnStreamOnline += StreamOnline;
        MonitoringService.OnStreamUpdate += StreamUpdate;
        MonitoringService.OnStreamOffline += StreamOffline;

        MonitoringService.Start();
    }

    public static void Reset()
    {
        StreamData.Clear();
        List<ChannelHandler.Channel> Channels = ChannelHandler.FetchedChannels;
        MonitoringService.SetChannelsByName(Channels.Select(x => x.Name).ToList());
    }

    public static void Stop() { MonitoringService.Stop(); }
    #endregion

    #region Monitor events
    private static void StreamOffline(object? sender, OnStreamOfflineArgs e)
    {
        TimeSpan uptime = DateTime.Now - StreamData[e.Channel].Started.ToLocalTime();
        string uptimeString = $"{uptime:h'h'm'm's's'}";
        StreamData[e.Channel] = new Stream(e.Channel, false, e.Stream.Title, e.Stream.GameName, DateTime.Now);
        MessageHandler.SendColoredMessage(
            Config.RelayChannel,
            $"{RandomReplies.StreamOfflineEmotes.Choice()} @{e.Channel} is now offline! {uptimeString}",
            ChatColor.GoldenRod);
    }

    private static void StreamUpdate(object? sender, OnStreamUpdateArgs e)
    {
        Stream current = new Stream(e.Channel, false, e.Stream.Title, e.Stream.GameName, e.Stream.StartedAt);
        if (StreamData[e.Channel].Title != e.Stream.Title
        || StreamData[e.Channel].GameName != e.Stream.GameName)
        {
            StreamData[e.Channel] = current;
            MessageHandler.SendColoredMessage(
                Config.RelayChannel,
                $"{RandomReplies.StreamUpdateEmotes.Choice()} @{e.Channel} updated their stream: {e.Stream.Title} -- {e.Stream.GameName}",
                ChatColor.DodgerBlue);
        }
    }

    private static void StreamOnline(object? sender, OnStreamOnlineArgs e)
    {
        Stream current = new Stream(e.Channel, false, e.Stream.Title, e.Stream.GameName, e.Stream.StartedAt);
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

    internal record Stream(string Username, bool IsOnline, string Title, string GameName, DateTime Started);
}
