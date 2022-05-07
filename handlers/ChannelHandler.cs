using _26tack_rewritten.core;
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
        await c.ForEachAsync(async x =>
        {
            if (x.priority >= 50)
            {
                MainClient.Client.JoinChannel(x.name);
                Log.Debug($"[Main] Joined: {x.name} (JustLog:{MainClient.JLChannels.Contains(x.name)})");
            }
            // TODO: Anon client
            Log.Debug($"[Anon] Joined: {x.name}");
            await Task.Delay(550);
        });

        MainClient.Client.OnFailureToReceiveJoinConfirmation += (s, e) =>
        {
            Log.Warning($"[Main] Failed to join: {e.Exception.Channel}, {e.Exception.Details}");
            JoinFailureChannels.Add(FetchedChannels.First(x => x.name == e.Exception.Channel));
        };
        MainClient.Client.OnJoinedChannel += (s, e) => MainJoinedChannels.Add(e.Channel);
        MainClient.Client.OnLeftChannel += (s, e) => MainJoinedChannels.Remove(e.Channel);
    }

    public static async Task<bool> JoinChannel(string channel, bool highPriority = false)
    {

        await Task.Delay(-1);
        return false;
    }

    private sealed record Channel(string name, string id, int priority, bool logged);
}
