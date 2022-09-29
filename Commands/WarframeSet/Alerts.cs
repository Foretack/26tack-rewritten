using System.Text;
using Tack.Database;
using Tack.Handlers;
using Tack.Json;
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

        Alert[] alerts = await "warframe:alerts".GetOrCreate<Alert[]>(async () =>
        {
            var r = await ExternalAPIHandler.WarframeStatusApi<Alert[]>("alerts");
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, ⚠ Request error! {r.Exception.Message}");
                return Array.Empty<Alert>();
            }
            return r.Value;
        }, true, TimeSpan.FromMinutes(2.5));
        if (alerts.Length == 0) return;

        string[] rewards = alerts
            .Where(x => x.Active)
            .Select(x => $"{x.Mission.Faction} / {x.Mission.Type} [{x.Mission.Reward.AsString}] ")
            .ToArray();

        _ = ab.Append($"{"Alert".PluralizeWith(alerts.Length)} ➜ ")
            .Append(string.Join(" ● ", rewards));

        MessageHandler.SendColoredMessage(channel, $"@{user}, {ab}", ChatColor.Coral);
    }
}
