using _26tack_rewritten.core;
using _26tack_rewritten.database;
using Dasync.Collections;
using Serilog;

namespace _26tack_rewritten.handlers;
internal static class ChannelHandler
{
    public static List<string> MainJoinedChannels { get; } = new List<string>();
    public static List<string> AnonJoinedChannels { get; } = new List<string>();

    private static readonly List<Channel> FetchedChannels = new List<Channel>();
    private static readonly List<Channel> JoinFailureChannels = new List<Channel>();

    internal static async Task Connect(bool isReconnect)
    {
        if (isReconnect)
        {
            MainJoinedChannels.Clear();
            AnonJoinedChannels.Clear();
        }
        IAsyncEnumerable<Channel> c = new AsyncEnumerable<Channel>(async y =>
        {
            for (int i = 0; i < FetchedChannels.Count; i++) await y.ReturnAsync(FetchedChannels[i]);
            y.Break();
        });
        await c.ForEachAsync((async x =>
        {
            if (x.Priority >= 50)
            {
                MainClient.Client.JoinChannel(x.Name);
                Log.Debug($"[Main] Joined: {x.Name} (JustLog:{MainClient.JLChannels.Contains(x.Name)})");
            }
            AnonymousClient.Client.JoinChannel(x.Name);
            Log.Debug($"[Anon] Joined: {x.Name}");
            await Task.Delay(550);
        }));

        MainClient.Client.OnFailureToReceiveJoinConfirmation += (s, e) =>
        {
            Log.Warning($"[Main] Failed to join: {e.Exception.Channel}, {e.Exception.Details}");
            JoinFailureChannels.Add(FetchedChannels.First(x => x.Name == e.Exception.Channel));
        };
        MainClient.Client.OnJoinedChannel += (s, e) => MainJoinedChannels.Add(e.Channel);
        MainClient.Client.OnLeftChannel += (s, e) => MainJoinedChannels.Remove(e.Channel);
        AnonymousClient.Client.OnFailureToReceiveJoinConfirmation += (s, e) =>
        {
            Log.Warning($"[Anon] Failed to join: {e.Exception.Channel}, {e.Exception.Details}");
        };
        AnonymousClient.Client.OnJoinedChannel += (s, e) => AnonJoinedChannels.Add(e.Channel);
        AnonymousClient.Client.OnLeftChannel += (s, e) => AnonJoinedChannels.Remove(e.Channel);
    }

    public static async Task<bool> JoinChannel(string channel, bool highPriority = false, bool logged = true)
    {
        bool x = false;
        bool y = false;
        if (highPriority)
        {
            MainClient.Client.JoinChannel(channel);
        }
        AnonymousClient.Client.JoinChannel(channel);
        MainClient.Client.OnJoinedChannel += (s, e) => { if (e.Channel == channel) x = true; };
        AnonymousClient.Client.OnJoinedChannel += (s, e) => { if (e.Channel == channel) y = true; };
        await Task.Delay(250);
        
        if (!(x && y)) return false;
        Database db = new Database();
        Channel c = new Channel(channel, "0", highPriority ? 50 : 0, logged); // TODO: Fetch channel ID
        await db.AddChannel(c);
        
        await Task.Delay(-1);
        return false;
    }

    public record Channel(string Name, string ID, int Priority, bool Logged);
}
