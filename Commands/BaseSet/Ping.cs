using Tack.Handlers;
using Tack.Misc;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;
using C = Tack.Core.Core;

namespace Tack.Commands.BaseSet;
internal sealed class Ping : Command
{
    public override CommandInfo Info { get; } = new(
        name: "ping",
        description: "Does the pong thing or whatever!",
        aliases: new string[] { "pong", "peng", "pang", "pung" }
    );

    public override Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        double latency = DateTimeOffset.Now.ToUnixTimeMilliseconds() - double.Parse(ctx.IrcMessage.TmiSentTs);
        TimeSpan uptime = DateTime.Now - C.StartupTime;

        MessageHandler.SendMessage(channel, $"{string.Join($" {user} ", RandomReplies.PingReplies.Choice())} " +
            $"● {latency}ms " +
            $"■ Uptime: {uptime.FormatTimeLeft()}");
        return Task.CompletedTask;
    }
}
