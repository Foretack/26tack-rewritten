using _26tack_rewritten.core;
using _26tack_rewritten.database;
using _26tack_rewritten.models;
using Dasync.Collections;
using Serilog;
using TwitchLib.Client.Events;

namespace _26tack_rewritten.handlers;
internal static class ChannelHandler
{
    public static List<Channel> MainJoinedChannels { get; } = new List<Channel>();
    public static List<string> MainJoinedChannelNames { get; } = new List<string>();
    public static List<Channel> AnonJoinedChannels { get; } = new List<Channel>();
    public static string[] JLChannels { get; private set;  } = Array.Empty<string>();

    private static readonly Database Db = new Database();
    private static readonly List<Channel> FetchedChannels = new List<Channel>(Db.GetChannels().Result);
    private static readonly List<Channel> JoinFailureChannels = new List<Channel>();

    internal static async Task Connect(bool isReconnect)
    {
        if (isReconnect)
        {
            MainJoinedChannels.Clear();
            MainJoinedChannelNames.Clear();
            AnonJoinedChannels.Clear();
        }
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
    }

    public static async Task<bool> JoinChannel(string channel, bool highPriority = false, bool logged = true)
    {
        Channel c = new Channel(channel, highPriority ? 50 : 0, logged);
        FetchedChannels.Add(c);

        if (highPriority) MainClient.Client.JoinChannel(channel);
        AnonymousClient.Client.JoinChannel(channel);

        Database db = new Database();
        UserFactory uf = new UserFactory();
        ExtendedChannel? ec = await uf.CreateChannelProfile(channel);
        if (ec is null) return false;
        await db.AddChannel(ec);
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
        Log.Debug($"[Anon] Left channel {e.Channel}");
        AnonJoinedChannels.Remove(FetchedChannels.First(x => x.Name == e.Channel));
    }

    // This will crash if FetchedChannels doesn't have the channel
    private static void AnonOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        Log.Debug($"[Anon] Joined channel {e.Channel}");
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
        Log.Debug($"[Main] Left channel {e.Channel}");
        MainJoinedChannels.Remove(FetchedChannels.First(x => x.Name == e.Channel));
        MainJoinedChannelNames.Remove(e.Channel);
    }

    private static void MainOnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        Log.Debug($"[Main] Joined channel {e.Channel}");
        MainJoinedChannels.Add(FetchedChannels.First(x => x.Name == e.Channel));
        MainJoinedChannelNames.Add(e.Channel);
    }

    public record Channel(string Name, int Priority, bool Logged);
}
