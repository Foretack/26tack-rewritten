using Tack.Handlers;
using Tack.Interfaces;
using Tack.Json;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal class Invasions : IChatCommand
{
    public Command Info() => new(
        name: "invasions",
        description: "Get the total rewards of ongoing invasions",
        cooldowns: new int[] { 5, 3 }
        );

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        InvasionNode[]? invasionNodes = ObjectCache.Get<InvasionNode[]>("invasions_wf")
            ?? await ExternalAPIHandler.GetInvasions();
        if (invasionNodes is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Failed to fetch current invasions :(");
            return;
        }
        string message = await SumItems(invasionNodes);
        MessageHandler.SendMessage(channel, $"@{user}, Total rewards of ongoing invasions: {message}");
        ObjectCache.Put("invasions_wf", invasionNodes, 300);
    }

    private async Task<string> SumItems(InvasionNode[] invasions)
    {
        Dictionary<string, int> allItems = await Task.Run(() =>
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (InvasionNode node in invasions)
            {
                CountedItem[] items = node.attackerReward.countedItems
                    .Concat(node.defenderReward.countedItems).ToArray();

                foreach (string item in items.Select(x => x.key))
                {
                    bool s = dict.TryAdd(item, 1);
                    if (!s) dict[item] += 1;
                }
            }

            return dict;
        });

        return string.Join(' ', allItems) + " R)";
    }
}
