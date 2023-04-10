using Tack.Core;
using Tack.Handlers;
using Tack.Misc;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.BaseSet;
internal sealed class Ping : Command
{
    public override CommandInfo Info { get; } = new(
        name: "ping",
        description: "Does the pong thing or whatever!",
        aliases: new string[] { "pong", "peng", "pang", "pung" }
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.Message.Author.DisplayName;
        string channel = ctx.Message.Channel.Name;
        TimeSpan latency = DateTimeOffset.Now - ctx.Message.SentTimestamp;
        string uptime = Time.SinceString(Program.StartupTime);

        await MessageHandler.SendMessage(channel,
            $"{string.Join($" {user} ", RandomReplies.PingReplies.Choice())} "
            + $"● {latency.TotalMilliseconds}ms "
            + $"● Uptime: {uptime}"
            + $"● M:{SingleOf<MainClient>.Obj.Client.JoinedChannels.Count}," +
                $"A:{SingleOf<AnonymousClient>.Obj.Client.JoinedChannels.Count}," +
                $"UP:{new DateTimeOffset(Program.StartupTime).ToUnixTimeMilliseconds}");
    }
}
