using _26tack_rewritten.core;
using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.misc;
using _26tack_rewritten.models;
using _26tack_rewritten.utils;

namespace _26tack_rewritten.commands.baseset;
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
        TimeSpan uptime = DateTime.Now - MainClient.StartupTime;
        string uptimeString = uptime.TotalDays >= 1 ? $"{uptime:d'd 'h'h '}" : $"{uptime:h'h 'm'm 's's '}";

        MessageHandler.SendMessage(channel, $"{string.Join($" {user} ", RandomReplies.PingReplies.Choice())} {latency}ms -- {uptimeString}");
        return Task.CompletedTask;
    }
}
