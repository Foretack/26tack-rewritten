using Tack.Handlers;
using Tack.Interfaces;
using Tack.Json;
using Tack.Models;

namespace Tack.Commands.WarframeSet;
internal class Drops : IChatCommand
{
    public Command Info()
    {
        string name = "drops";
        string description = "Get the best location to farm a specific item";
        string[] aliases = { "where" };
        int[] cooldowns = { 10, 3 };

        return new Command(name, description, aliases, cooldowns);
    }

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, You have to specify an item xd!");
            return;
        }
        // The api doesn't return the drop locations for relics, but rather their contents for some reason.
        string[] forbidden = { "lith", "meso", "neo", "axi", "requiem" };
        if (forbidden.Contains(args[0].ToLower()))
        {
            MessageHandler.SendMessage(channel, $"@{user}, Relic drop locations are not supported.");
            return;
        }

        string item = string.Join(' ', ctx.Args);
        ItemDropData[]? itemDrops = await ExternalAPIHandler.GetItemDropData(item);
        if (itemDrops is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, An error occured with your request :(");
            return;
        }
        if (itemDrops.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, No drop locations for that item were found. Unlucky!");
            return;
        }

        ItemDropData[] topDrops = await Task.Run(() =>
        {
            return itemDrops.OrderByDescending(x => x.chance).ToArray();
        });
        if (topDrops.Length > 3) topDrops = topDrops[..3];
        string[] dropsString = topDrops.Select(x => $"{x.place} 🡺 {x.chance}%").ToArray();
        MessageHandler.SendMessage(channel, $"@{user}, Top drop locations for \"{topDrops[0].item}\": " +
            string.Join(" -- ", dropsString));
    }
}
