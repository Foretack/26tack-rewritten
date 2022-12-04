using System.Text;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal sealed class Alerts : Command
{
    public override CommandInfo Info { get; } = new(
        name: "alerts",
        description: "Get the current alerts in the system",
        userCooldown: 10,
        channelCooldown: 3
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        var ab = new StringBuilder();

        var alertsCache = await Redis.Cache.TryGetObjectAsync<Alert[]>("warframe:alerts");
        if (!alertsCache.keyExists)
        {
            var r = await ExternalAPIHandler.WarframeStatusApi<Alert[]>("alerts");
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, ⚠ Request failed: {r.Exception.Message}");
                return;
            }
            await Redis.Cache.SetObjectAsync("warframe:alerts", r.Value, TimeSpan.FromMinutes(2.5));
            alertsCache.value = r.Value;
        }
        Alert[] alerts = alertsCache.value;

        string[] rewards = alerts
            .Where(x => x.Active)
            .Select(x => $"{x.Mission.Faction} / {x.Mission.Type} [{x.Mission.Reward.AsString}] ")
            .ToArray();

        _ = ab.Append($"{"Alert".PluralizeWith(alerts.Length)} ➜ ")
            .Append(string.Join(" ● ", rewards));

        MessageHandler.SendColoredMessage(channel, $"@{user}, {ab}", ChatColor.Coral);
    }
}
