using Tack.Database;
using Tack.Handlers;
using Tack.Json;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Commands.WarframeSet;
internal sealed class Invasions : Command
{
    public override CommandInfo Info { get; } = new(
        name: "invasions",
        description: "Get the total rewards of ongoing invasions",
        userCooldown: 5,
        channelCooldown: 3
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        InvasionNode[] invasionNodes = await "warframe:invasions".GetOrCreate<InvasionNode[]>(async () =>
        {
            var r = await ExternalAPIHandler.WarframeStatusApi<InvasionNode[]>("invasions");
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, Failed to fetch current invasions :( ({r.Exception.Message})");
                return default!;
            }
            return r.Value;
        }, true, TimeSpan.FromMinutes(5));
        if (invasionNodes is null) return;

        string message = await SumItems(invasionNodes);
        MessageHandler.SendMessage(channel, $"@{user}, Total rewards of ongoing invasions: {message}");
    }

    private async Task<string> SumItems(InvasionNode[] invasions)
    {
        var allItems = await Task.Run(() =>
        {
            var dict = new Dictionary<string, int>();
            foreach (InvasionNode node in invasions)
            {
                CountedItem[] items = node.AttackerReward.CountedItems
                    .Concat(node.DefenderReward.CountedItems).ToArray();

                foreach (string item in items.Select(x => x.Key))
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
