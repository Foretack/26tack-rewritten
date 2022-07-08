using Tack.Handlers;
using Tack.Interfaces;
using Tack.Misc;
using Tack.Models;
using Tack.Utils;
using C = Tack.Core.Core;

namespace Tack.Commands.BaseSet;
internal class Ping : IChatCommand
{
    public Command Info()
    {
        string name = "ping";
        string description = "Does the pong thing or whatever!";
        string[] aliases = { "pong", "peng", "pang", "pung" };
        return new Command(name, description, aliases);
    }

    public Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        double latency = DateTimeOffset.Now.ToUnixTimeMilliseconds() - double.Parse(ctx.IrcMessage.TmiSentTs);
        TimeSpan uptime = DateTime.Now - C.StartupTime;

        MessageHandler.SendMessage(channel, $"{string.Join($" {user} ", RandomReplies.PingReplies.Choice())} {latency}ms " +
            $"◆ Memory: {C.GetMemoryUsage()}MB " +
            $"◆ Uptime: {uptime.FormatTimeLeft()}");
        return Task.CompletedTask;
    }
}
