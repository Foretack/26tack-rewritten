using System.Text;
using Tack.Handlers;
using Tack.Json;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal class Alerts : Command
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

        Alert[]? alerts = ObjectCache.Get<Alert[]>("alerts_wf");
        if (alerts is null)
        {
            Result<Alert[]> r = await ExternalAPIHandler.WarframeStatusApi<Alert[]>("alerts");
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, ⚠ Http error! {r.Exception.Message}");
                return;
            }
            alerts = r.Value;
        }

        string[] rewards = alerts
            .Where(x => x.active)
            .Select(x => $"{x.mission.faction} / {x.mission.type} [{x.mission.reward.asString}] ")
            .ToArray();

        _ = ab.Append($"{"Alert".PluralizeWith(alerts.Length)} ➜ ")
            .Append(string.Join(" ● ", rewards));

        MessageHandler.SendColoredMessage(channel, $"@{user}, {ab}", ChatColor.Coral);
        ObjectCache.Put("alerts_wf", alerts, 150);
    }
}
