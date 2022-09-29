﻿using Tack.Database;
using Tack.Handlers;
using Tack.Json;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal sealed class Cycle : Command
{
    public override CommandInfo Info { get; } = new(
        name: "cycle",
        description: "Get the current cycle of the specified open-world node",
        aliases: new string[] { "cycles", "cetus", "vallis", "cambion", "drift", "zariman" },
        userCooldown: 5,
        channelCooldown: 3
    );

    private string[] CycleTypes => Info.Aliases;
    public override async Task Execute(CommandContext ctx)
    {
        if (!CycleTypes.Contains(ctx.CommandName))
        {
            await Other(ctx);
            return;
        }
        await SendCycleOf(ctx);
    }

    private async ValueTask SendCycle<T>(CommandContext ctx) where T : IWorldCycle, new()
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string queryString = new T().QueryString;

        T cycle = await $"warframe:cycles:{queryString}".GetOrCreate<T>(async () =>
        {
            var r = await ExternalAPIHandler.WarframeStatusApi<T>(queryString);
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, ⚠ Request error! {r.Exception.Message}");
                return default!;
            }
            return r.Value;
        }, true);
        if (cycle is null) return;

        TimeSpan timeLeft = Time.Until(cycle.Expiry);
        if (timeLeft.TotalMilliseconds < 0)
        {
            await $"warframe:cycles:{queryString}".RemoveKey();
            MessageHandler.SendMessage(channel, $"@{user}, Cycle data is outdated. Try again later?");
            return;
        }
        await $"warframe:cycles:{queryString}".SetKeyExpiry(timeLeft);

        MessageHandler.SendMessage(channel, $"@{user}, {cycle.State} | time left: {(timeLeft).FormatTimeLeft()}");
    }

    private async ValueTask Other(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, FeelsDankMan specify which cycle you want {CycleTypes.AsString()}");
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
                await SendCycle<VallisCycle>(ctx);
                break;
            case "cambion":
            case "drift":
                await SendCycle<CambionCycle>(ctx);
                break;
            case "zariman":
                await SendCycle<ZarimanCycle>(ctx);
                break;
            default:
                await SendCycle<CetusCycle>(ctx);
                break;
        }
    }
}
