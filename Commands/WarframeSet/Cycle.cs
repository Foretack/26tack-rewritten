using Tack.Handlers;
using Tack.Nonclass;
using Tack.Json;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal class Cycle : IChatCommand
{
    private readonly string[] CycleTypes = { "cetus", "vallis", "cambion", "drift", "zariman" };
    public Command Info() => new(
        name: "cycle",
        description: "Get the current cycle of the specified open-world node",
        aliases: new string[] { "cycles", "cetus", "vallis", "cambion", "drift", "zariman" },
        cooldowns: new int[] { 5, 3 }
        );

    public async Task Run(CommandContext ctx)
    {
        if (!CycleTypes.Contains(ctx.CommandName))
        {
            await Other(ctx);
            return;
        }
        await SendCycleOf(ctx);
    }

    private async ValueTask SendCycle<T>(CommandContext ctx, string cycleName) where T : IWorldCycle
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        
        T? cycle = ObjectCache.Get<T>(cycleName + "_wf");
        if (cycle is null)
        {
            var r = await ExternalAPIHandler.WarframeStatusApi<T>(cycleName);
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, An unexpected error occured :( ({r.Exception.Message})");
                return;
            }
            cycle = r.Value;
        }
        TimeSpan timeLeft = cycle.Expiry.ToLocalTime() - DateTime.Now;
        if (timeLeft.TotalSeconds < 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Cycle data is outdated. Try again later?");
            return;
        }
        MessageHandler.SendMessage(channel, $"@{user}, {cycle.State} | time left: {timeLeft.FormatTimeLeft()}");
        ObjectCache.Put(cycleName + "_wf", cycle, (int)timeLeft.TotalSeconds);
    }

    private async ValueTask Other(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, FeelsDankMan specify which cycle you want <{string.Join('/', CycleTypes)}>");
            return;
        }
        if (!CycleTypes.Contains(args[0].ToLower()))
        {
            MessageHandler.SendMessage(channel, $"@{user}, FeelsDankMan idk what \"{args[0]}\" is");
            return;
        }

        await SendCycleOf(ctx, args[0]);
    }

    private async ValueTask SendCycleOf(CommandContext ctx, string? name = null)
    {
        string commandName = name ?? ctx.CommandName;

        switch (commandName)
        {
            case "vallis":
                await SendCycle<VallisCycle>(ctx, "vallisCycle");
                break;

            case "cambion":
                await SendCycle<CambionCycle>(ctx, "cambionCycle");
                break;

            case "drift":
                await SendCycle<CambionCycle>(ctx, "cambionCycle");
                break;

            case "zariman":
                await SendCycle<ZarimanCycle>(ctx, "zarimanCycle");
                break;

            default:
                await SendCycle<CetusCycle>(ctx, "cetusCycle");
                break;
        }
    }
}
