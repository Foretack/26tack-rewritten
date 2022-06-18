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

    public static List<Channel> MainJoinedChannels { get; } = new List<Channel>();
    public static List<string> MainJoinedChannelNames { get; } = new List<string>();
    public static List<Channel> AnonJoinedChannels { get; } = new List<Channel>();
    public static string[] JLChannels { get; private set; } = Array.Empty<string>();
    public static List<Channel> FetchedChannels { get; } = new List<Channel>(Db.GetChannels().Result);

    private static readonly List<Channel> JoinFailureChannels = new List<Channel>();

    internal static async Task Connect(bool isReconnect)
    {
        if (isReconnect)
        {
            MainJoinedChannels.Clear();
            MainJoinedChannelNames.Clear();
            AnonJoinedChannels.Clear();
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
        StreamMonitor.Start();
    }

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

    private static void AnonOnFailedJoin(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
    {
        Log.Warning($"[Anon] Failed to join {e.Exception.Channel}: {e.Exception.Details}");
    }

    private static void AnonOnLeftChannel(object? sender, OnLeftChannelArgs e)
    {
        Log.Information($"[Anon] Left channel {e.Channel}");
        AnonJoinedChannels.Remove(FetchedChannels.First(x => x.Name == e.Channel));
    }

    // This will crash if FetchedChannels doesn't have the channel
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

    public record Channel(string Name, int Priority, bool Logged);
}

internal static class StreamMonitor
{
    public static Dictionary<string, bool> StreamsStatus { get; private set; } = new Dictionary<string, bool>();

    private static readonly LiveStreamMonitorService MonitoringService = new LiveStreamMonitorService(TwitchAPIHandler.API, 30);
    private static readonly Dictionary<string, string[]> StreamsData = new Dictionary<string, string[]>();

    public static void Start()
    {
        List<string> channelsByName = ChannelHandler.FetchedChannels.Select(x => x.Name).ToList();
        StreamsStatus = channelsByName.ToDictionary(x => x, y => false);
        MonitoringService.SetChannelsByName(channelsByName);

        MonitoringService.OnServiceStarted += ServiceStarted;
        MonitoringService.OnStreamOnline += StreamOnline;
        MonitoringService.OnStreamUpdate += StreamUpdate;
        MonitoringService.OnStreamOffline += StreamOffline;

        MonitoringService.Start();
    }

    private static void StreamOffline(object? sender, OnStreamOfflineArgs e)
    {
        StreamsStatus[e.Channel] = false;
        MessageHandler.SendColoredMessage(
            Config.RelayChannel,
            $"{RandomReplies.StreamOfflineEmotes.Choice()} @{e.Channel} is now offline!",
            ChatColor.GoldenRod);
    }

    private static void StreamUpdate(object? sender, OnStreamUpdateArgs e)
    {
        bool s = StreamsData.TryAdd(e.Channel, new string[] { e.Stream.Title, e.Stream.GameName });
        if (s) return;
        if (StreamsData[e.Channel][0] != e.Stream.Title)
        {
            StreamsData[e.Channel][0] = e.Stream.Title;
            MessageHandler.SendColoredMessage(
                Config.RelayChannel,
                $"{RandomReplies.StreamUpdateEmotes.Choice()} @{e.Channel} changed their title: {e.Stream.Title}",
                ChatColor.DodgerBlue);
        }
        if (StreamsData[e.Channel][1] != e.Stream.GameName)
        {
            StreamsData[e.Channel][1] = e.Stream.GameName;
            MessageHandler.SendColoredMessage(
                Config.RelayChannel,
                $"{RandomReplies.StreamUpdateEmotes.Choice()} @{e.Channel} is now playing: {e.Stream.GameName}",
                ChatColor.DodgerBlue);
        }
    }

    private static void StreamOnline(object? sender, OnStreamOnlineArgs e)
    {
        StreamsStatus[e.Channel] = true;
        MessageHandler.SendColoredMessage(
            Config.RelayChannel,
            $"{RandomReplies.StreamOnlineEmotes.Choice()} @{e.Channel} has gone live: {e.Stream.Title} - {e.Stream.GameName}",
            ChatColor.SpringGreen);
    }

    private static void ServiceStarted(object? sender, OnServiceStartedArgs e)
    {
        MessageHandler.SendMessage(Config.RelayChannel, $"OBSOLETE Hello");
    }
}
