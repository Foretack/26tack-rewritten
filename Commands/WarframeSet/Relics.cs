﻿using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal sealed class Relics : Command
{
    public override CommandInfo Info { get; } = new(
        name: "relics",
        description: "Find which relics contain an item, or the contents of a specific relic",
        aliases: new string[] { "relic", "r" },
        userCooldown: 5,
        channelCooldown: 3
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.Message.Author.DisplayName;
        string channel = ctx.Message.Channel.Name;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            await MessageHandler.SendMessage(channel, $"@{user}, Specify an item or a Relic or something fdm");
            return;
        }

        bool relic = args[0].ToLower() switch
        {
            "lith" => true,
            "meso" => true,
            "neo" => true,
            "axi" => true,
            "requiem" => true,
            _ => false
        };

        string item = string.Join(' ', args).ToLower();
        string message;

        (bool keyExists, RelicData value) = await Redis.Cache.TryGetObjectAsync<RelicData>("warframe:relicdata");
        if (!keyExists)
        {
            RelicData? r = await ExternalApiHandler.GetRelicData();
            if (r is null)
            {
                await MessageHandler.SendMessage(channel, $"@{user}, Error getting relic information :(");
                return;
            }

            await Redis.Cache.SetObjectAsync("warframe:relicdata", r, TimeSpan.FromDays(1));
            value = r;
        }

        RelicData relicData = value;

        message = relic ? await GetRelicItems(item, relicData) : await FindRelicsForItem(item, relicData);
        await MessageHandler.SendMessage(channel, $"@{user}, {message}");
    }

    private async Task<string> FindRelicsForItem(string itemName, RelicData relicData)
    {
        string[] wantedRelics = await Task.Run(() =>
        {
            wantedRelics = relicData.Relics
                .Where(x => x.Rewards.Any(y => y.ItemName.ToLower().StartsWith(itemName)) && x.State == "Intact")
                .Select(x => x.Tier + ' ' + x.RelicName)
                .ToArray();

            return wantedRelics;
        });

        if (wantedRelics.Length == 0)
            return "No Relics containing that item were found.";
        else if (wantedRelics.Length >= 15)
            return "Too many Relics contain that item! (message too big)";

        return $"Relics containing \"{itemName}\": {wantedRelics.AsString()} 🥜";
    }
    private async Task<string> GetRelicItems(string relicName, RelicData relicData)
    {
        Relic[] relic = await Task.Run(() =>
        {
            return relicData.Relics
            .Where(x => (x.Tier + ' ' + x.RelicName).ToLower() == relicName)
            .ToArray();
        });

        if (relic.Length == 0)
            return "That Relic was not found!";

        Relic r = relic[0];
        return $"Contents of \"{r.Tier} {r.RelicName}\": " +
            $"[{string.Join(" | ", r.Rewards.OrderByDescending(x => x.Chance).Select(x => x.ItemName))}] 🌰";
    }
}
