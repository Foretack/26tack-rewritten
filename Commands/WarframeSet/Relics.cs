using Tack.Handlers;
using Tack.Nonclass;
using Tack.Json;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal class Relics : IChatCommand
{
    public Command Info() => new(
        name: "relics",
        description: "Find which relics contain an item, or the contents of a specific relic",
        aliases: new string[] { "relic", "r" },
        cooldowns: new int[] { 5, 3 }
        );

    public async Task Run(CommandContext ctx)
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
            "lith"      => true,
            "meso"      => true,
            "neo"       => true,
            "axi"       => true,
            "requiem"   => true,
            _           => false
        };

        string item = string.Join(' ', args).ToLower();
        string message;
        RelicData? relicData = ObjectCache.Get<RelicData>("relics_wf")
            ?? await ExternalAPIHandler.GetRelicData();
        if (relicData is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, There was an error while trying to get relic information :(");
            return;
        }
        message = relic ? await GetRelicItems(item, relicData) : await FindRelicsForItem(item, relicData);
        MessageHandler.SendMessage(channel, $"@{user}, {message}");
        ObjectCache.Put("relics_wf", relicData, 3600);
    }

    private async Task<string> FindRelicsForItem(string itemName, RelicData relicData)
    {
        string[] wantedRelics = await Task.Run(() =>
        {
            wantedRelics = relicData.relics
                .Where(x => x.rewards.Any(y => y.itemName.ToLower().StartsWith(itemName)) && x.state == "Intact")
                .Select(x => x.tier + ' ' + x.relicName)
                .ToArray();

            return wantedRelics;
        });

        if (wantedRelics.Length == 0) return "No Relics containing that item were found.";
        if (wantedRelics.Length >= 15) return "Too many Relics contain that item! (message too big)";
        return $"Relics containing \"{itemName}\": [{string.Join(" | ", wantedRelics)}] 🥜";
    }
    private async Task<string> GetRelicItems(string relicName, RelicData relicData)
    {
        Relic[] relic = await Task.Run(() =>
        {
            return relicData.relics
            .Where(x => (x.tier + ' ' + x.relicName).ToLower() == relicName)
            .ToArray();
        });

        if (relic.Length == 0) return "That Relic was not found!";
        
        Relic r = relic[0];
        return $"Contents of \"{r.tier} {r.relicName}\": " +
            $"[{string.Join(" | ", r.rewards.OrderByDescending(x => x.chance).Select(x => x.itemName))}] 🌰";
    }
}
