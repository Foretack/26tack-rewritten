using Tack.Core;
using Tack.Database;
using Tack.Models;
using Dasync.Collections;
using Serilog;
using TwitchLib.Client.Events;

namespace Tack.Handlers;
internal static class ChannelHandler
{
    public static List<Channel> MainJoinedChannels { get; } = new List<Channel>();
    public static List<string> MainJoinedChannelNames { get; } = new List<string>();
    public static List<Channel> AnonJoinedChannels { get; } = new List<Channel>();
    public static string[] JLChannels { get; private set;  } = Array.Empty<string>();

    private static readonly Database.Database Db = new Database.Database();
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
