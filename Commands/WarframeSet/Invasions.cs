using Tack.Database;
using Tack.Handlers;
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
        string user = ctx.Message.Author.DisplayName;
        string channel = ctx.Message.Channel.Name;

        (bool keyExists, InvasionNode[] value) = await Redis.Cache.TryGetObjectAsync<InvasionNode[]>("warframe:invasions");
        if (!keyExists)
        {
            Result<InvasionNode[]> r = await ExternalApiHandler.WarframeStatusApi<InvasionNode[]>("invasions");
            if (!r.Success)
            {
                await MessageHandler.SendMessage(channel, $"@{user}, ⚠ Request failed: {r.Exception.Message}");
                return;
            }

            await Redis.Cache.SetObjectAsync("warframe:invasions", r.Value, TimeSpan.FromMinutes(5));
            value = r.Value;
        }

        InvasionNode[] invasionNodes = value;

        string message = await SumItems(invasionNodes);
        await MessageHandler.SendMessage(channel, $"@{user}, Total rewards of ongoing invasions: {message}");
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
                    if (!s)
                        dict[item] += 1;
                }
            }

            return dict;
        });

        return string.Join(' ', allItems) + " R)";
    }
}
