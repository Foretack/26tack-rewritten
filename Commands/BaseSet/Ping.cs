using Tack.Core;
using Tack.Database;
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

    public Ping()
    {
        Time.DoEvery(30, () => RedisPing());
    }

    public override Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        double latency = DateTimeOffset.Now.ToUnixTimeMilliseconds() - double.Parse(ctx.IrcMessage.TmiSentTs);
        string uptime = Time.SinceString(Program.StartupTime);
        string shardStatus = AnonymousClient.ShardStatus;

        MessageHandler.SendMessage(channel, $"{string.Join($" {user} ", RandomReplies.PingReplies.Choice())} " +
            $"● {latency}ms " +
            $"● Uptime: {uptime} ● Shard status: {shardStatus}");

        return Task.CompletedTask;
    }

    private async Task RedisPing()
    {
        await Redis.PublishAsync("shard:manage", "PING");
    }
}
