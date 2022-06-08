using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.json;
using _26tack_rewritten.models;
using _26tack_rewritten.utils;

namespace _26tack_rewritten.commands.warframeset;
internal class Cycle : DataCacher<object>, IChatCommand
{
    public Command Info()
    {
        string name = "cycle";
        string description = "Get the current cycle of the specified open-world node";
        string[] aliases = { "cycles", "cetus", "vallis", "cambion", "drift" };
        int[] cooldowns = { 10, 3 };

        return new Command(name, description, aliases, cooldowns);
    }

    public async Task Run(CommandContext ctx)
    {
        ValueTask task(CommandContext ctx) => ctx.CommandName switch
        {
            "cetus" => SendCetusCycle(ctx),
            "vallis" => SendVallisCycle(ctx),
            "cambion" => SendCambionCycle(ctx),
            "drift" => SendCambionCycle(ctx),
            _ => Other(ctx)
        };

        await task(ctx);
    }

    private async ValueTask SendCetusCycle(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        CetusCycle? cycle = (CetusCycle?)GetCachedPiece("cetus")?.Object;
        if (cycle is not null)
        {
            TimeSpan timeLeft = cycle.expiry.ToLocalTime() - DateTime.Now.ToLocalTime();
            if (timeLeft.TotalSeconds < 0)
            {
                MessageHandler.SendMessage(channel, $"@{user}, Cycle data is outdated. Try again later?");
                return;
            }
            string timeLeftString = timeLeft.TotalHours < 1 ? $"{timeLeft:m'm's's'}" : $"{timeLeft:h'h'm'm's's'}";
            MessageHandler.SendMessage(channel, $"@{user}, {(cycle.isDay ? $"☀" : "🌙")} | time left: {timeLeftString}");
            return;
        }

        cycle = await ExternalAPIHandler.GetCetusCycle();
        if (cycle is not null)
        {
            TimeSpan timeLeft = cycle.expiry.ToLocalTime() - DateTime.Now.ToLocalTime();
            if (timeLeft.TotalSeconds < 0)
            {
                MessageHandler.SendMessage(channel, $"@{user}, Cycle data is outdated. Try again later?");
                return;
            }
            CachePiece("cetus", cycle, (int)timeLeft.TotalSeconds);
            string timeLeftString = timeLeft.TotalHours < 1 ? $"{timeLeft:m'm's's'}" : $"{timeLeft:h'h'm'm's's'}";
            MessageHandler.SendMessage(channel, $"@{user}, {(cycle.isDay ? '☀' : "🌙")} | time left: {timeLeftString}");
            return;
        }

        MessageHandler.SendMessage(channel, $"@{user}, An unexpected error occured :(");
    }
    private async ValueTask SendVallisCycle(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        VallisCycle? cycle = (VallisCycle?)GetCachedPiece("vallis")?.Object;
        if (cycle is not null)
        {
            TimeSpan timeLeft = cycle.expiry.ToLocalTime() - DateTime.Now.ToLocalTime();
            if (timeLeft.TotalSeconds < 0)
            {
                MessageHandler.SendMessage(channel, $"@{user}, Cycle data is outdated. Try again later?");
                return;
            }
            string timeLeftString = timeLeft.TotalHours < 1 ? $"{timeLeft:m'm's's'}" : $"{timeLeft:h'h'm'm's's'}";
            MessageHandler.SendMessage(channel, $"@{user}, {(cycle.isWarm ? "🔥" : '❄')} | time left: {timeLeftString}");
            return;
        }

        cycle = await ExternalAPIHandler.GetVallisCycle();
        if (cycle is not null)
        {
            TimeSpan timeLeft = cycle.expiry.ToLocalTime() - DateTime.Now.ToLocalTime();
            if (timeLeft.TotalSeconds < 0)
            {
                MessageHandler.SendMessage(channel, $"@{user}, Cycle data is outdated. Try again later?");
                return;
            }
            CachePiece("vallis", cycle, (int)timeLeft.TotalSeconds);
            string timeLeftString = timeLeft.TotalHours < 1 ? $"{timeLeft:m'm's's'}" : $"{timeLeft:h'h'm'm's's'}";
            MessageHandler.SendMessage(channel, $"@{user}, {(cycle.isWarm ? "🔥" : '❄')} | time left: {timeLeftString}");
            return;
        }

        MessageHandler.SendMessage(channel, $"@{user}, An unexpected error occured :(");
    }
    private async ValueTask SendCambionCycle(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        CambionCycle? cycle = (CambionCycle?)GetCachedPiece("cambion")?.Object;
        if (cycle is not null)
        {
            TimeSpan timeLeft = cycle.expiry.ToLocalTime() - DateTime.Now.ToLocalTime();
            if (timeLeft.TotalSeconds < 0)
            {
                MessageHandler.SendMessage(channel, $"@{user}, Cycle data is outdated. Try again later?");
                return;
            }
            string timeLeftString = timeLeft.TotalHours < 1 ? $"{timeLeft:m'm's's'}" : $"{timeLeft:h'h'm'm's's'}";
            MessageHandler.SendMessage(channel, $"@{user}, {cycle.active} | time left: {timeLeftString}");
            return;
        }

        cycle = await ExternalAPIHandler.GetCambionCycle();
        if (cycle is not null)
        {
            TimeSpan timeLeft = cycle.expiry.ToLocalTime() - DateTime.Now.ToLocalTime();
            if (timeLeft.TotalSeconds < 0)
            {
                MessageHandler.SendMessage(channel, $"@{user}, Cycle data is outdated. Try again later?");
                return;
            }
            CachePiece("cambion", cycle, (int)timeLeft.TotalSeconds);
            string timeLeftString = timeLeft.TotalHours < 1 ? $"{timeLeft:m'm's's'}" : $"{timeLeft:h'h'm'm's's'}";
            MessageHandler.SendMessage(channel, $"@{user}, {cycle.active} | time left: {timeLeftString}");
            return;
        }

        MessageHandler.SendMessage(channel, $"@{user}, An unexpected error occured :(");
    }
    private async ValueTask Other(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, FeelsDankMan specify which cycle you want <cetus/vallis/cambion>");
            return;
        }

        ValueTask task(CommandContext ctx) => args[0].ToLower() switch
        {
            "cetus" => SendCetusCycle(ctx),
            "vallis" => SendVallisCycle(ctx),
            "cambion" => SendCambionCycle(ctx),
            "drift" => SendCambionCycle(ctx),
            _ => new Func<ValueTask>(() =>
            {
                MessageHandler.SendMessage(channel, $"@{user}, FeelsDankMan idk what \"{args[0]}\" is");
                return new ValueTask();
            }).Invoke()
        };

        await task(ctx);
    }
}
