using Tack.Handlers;
using Tack.Interfaces;
using Tack.Json;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal class Cycle : IChatCommand
{
    public Command Info()
    {
        string name = "cycle";
        string description = "Get the current cycle of the specified open-world node";
        string[] aliases = { "cycles", "cetus", "vallis", "cambion", "drift", "zariman" };
        int[] cooldowns = { 5, 3 };

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
            "zariman" => SendZarimanCycle(ctx),
            _ => Other(ctx)
        };

        await task(ctx);
    }

    private async ValueTask SendCetusCycle(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        CetusCycle? cycle = ObjectCache.Get<CetusCycle>("cetus_state_wf")
            ?? await ExternalAPIHandler.GetCetusCycle();
        if (cycle is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, An unexpected error occured :(");
            return;
        }
        TimeSpan timeLeft = cycle.expiry.ToLocalTime() - DateTime.Now.ToLocalTime();
        if (timeLeft.TotalSeconds < 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Cycle data is outdated. Try again later?");
            return;
        }
        string timeLeftString = timeLeft.TotalHours < 1 ? $"{timeLeft:m'm's's'}" : $"{timeLeft:h'h'm'm's's'}";
        MessageHandler.SendMessage(channel, $"@{user}, {(cycle.isDay ? $"☀" : "🌙")} | time left: {timeLeftString}");
        ObjectCache.Put("cetus_state_wf", cycle, (int)timeLeft.TotalSeconds);
    }
    private async ValueTask SendVallisCycle(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        VallisCycle? cycle = ObjectCache.Get<VallisCycle>("vallis_state_wf")
            ?? await ExternalAPIHandler.GetVallisCycle();
        if (cycle is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, An unexpected error occured :(");
            return;
        }
        TimeSpan timeLeft = cycle.expiry.ToLocalTime() - DateTime.Now.ToLocalTime();
        if (timeLeft.TotalSeconds < 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Cycle data is outdated. Try again later?");
            return;
        }
        string timeLeftString = timeLeft.TotalHours < 1 ? $"{timeLeft:m'm's's'}" : $"{timeLeft:h'h'm'm's's'}";
        MessageHandler.SendMessage(channel, $"@{user}, {(cycle.isWarm ? "🔥" : '❄')} | time left: {timeLeftString}");
        ObjectCache.Put("vallis_state_wf", cycle, (int)timeLeft.TotalSeconds);
    }
    private async ValueTask SendCambionCycle(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        CambionCycle? cycle = ObjectCache.Get<CambionCycle>("cambion_state_wf")
            ?? await ExternalAPIHandler.GetCambionCycle();
        if (cycle is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, An unexpected error occured :(");
            return;
        }
        TimeSpan timeLeft = cycle.expiry.ToLocalTime() - DateTime.Now.ToLocalTime();
        if (timeLeft.TotalSeconds < 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Cycle data is outdated. Try again later?");
            return;
        }
        string timeLeftString = timeLeft.TotalHours < 1 ? $"{timeLeft:m'm's's'}" : $"{timeLeft:h'h'm'm's's'}";
        MessageHandler.SendMessage(channel, $"@{user}, {cycle.active} | time left: {timeLeftString}");
        ObjectCache.Put("cambion_state_wf", cycle, (int)timeLeft.TotalSeconds);
    }
    private async ValueTask SendZarimanCycle(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        ZarimanCycle? cycle = ObjectCache.Get<ZarimanCycle>("zariman_state_wf")
            ?? await ExternalAPIHandler.GetZarimanCycle();
        if (cycle is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, An unexpected error occured :(");
            return;
        }
        TimeSpan timeLeft = cycle.Expiry.ToLocalTime() - DateTime.Now.ToLocalTime();
        if (timeLeft.TotalSeconds < 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Cycle data is outdated. Try again later?");
            return;
        }
        string timeLeftString = timeLeft.TotalHours < 1 ? $"{timeLeft:m'm's's'}" : $"{timeLeft:h'h'm'm's's'}";
        MessageHandler.SendMessage(channel, $"@{user}, {cycle.State} | time left: {timeLeftString}");
        ObjectCache.Put("zariman_state_wf", cycle, (int)timeLeft.TotalSeconds);
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
            "zariman" => SendZarimanCycle(ctx),
            _ => new Func<ValueTask>(() =>
            {
                MessageHandler.SendMessage(channel, $"@{user}, FeelsDankMan idk what \"{args[0]}\" is");
                return new ValueTask();
            }).Invoke()
        };

        await task(ctx);
    }
}
