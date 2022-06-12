using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.json;
using _26tack_rewritten.models;
using _26tack_rewritten.utils;

namespace _26tack_rewritten.commands.warframeset;
internal class Invasions : DataCacher<InvasionNode[]>, IChatCommand
{
    public Command Info()
    {
        string name = "invasions";
        string description = "Get the total rewards of ongoing invasions";
        int[] cooldowns = { 10, 3 };

        return new Command(name, description, cooldowns: cooldowns);
    }

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        InvasionNode[]? invasionNodes = GetCachedPiece("invasions")?.Object
            ?? await ExternalAPIHandler.GetInvasions();
        if (invasionNodes is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Failed to fetch current invasions :(");
            return;
        }
        string message = await SumItems(invasionNodes);
        MessageHandler.SendMessage(channel, $"@{user}, Total rewards of ongoing invasions: {message}");
        CachePiece("invasions", invasionNodes, 300);
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
