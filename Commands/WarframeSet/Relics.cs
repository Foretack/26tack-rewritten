using Tack.Database;
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
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Specify an item or a Relic or something fdm");
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
            RelicData? r = await ExternalAPIHandler.GetRelicData();
            if (r is null)
            {
                MessageHandler.SendMessage(channel, $"@{user}, Error getting relic information :(");
                return;
            }

            await Redis.Cache.SetObjectAsync("warframe:relicdata", r, TimeSpan.FromDays(1));
            value = r;
        }

        RelicData relicData = value;

        message = relic ? await GetRelicItems(item, relicData) : await FindRelicsForItem(item, relicData);
        MessageHandler.SendMessage(channel, $"@{user}, {message}");
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

        return wantedRelics.Length == 0
            ? "No Relics containing that item were found."
            : wantedRelics.Length >= 15
            ? "Too many Relics contain that item! (message too big)"
            : $"Relics containing \"{itemName}\": {wantedRelics.AsString()} 🥜";
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