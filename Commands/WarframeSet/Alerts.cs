using System.Text;
using Tack.Handlers;
using Tack.Nonclass;
using Tack.Json;
using Tack.Models;
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
        StringBuilder ab = new StringBuilder();

        Alert[]? alerts = ObjectCache.Get<Alert[]>("alerts_wf");
        if (alerts is null)
        {
            var r = await ExternalAPIHandler.WarframeStatusApi<Alert[]>("alerts");
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, There was an error retrieving alert data PoroSad ({r.Exception.Message})");
                return; 
            }
            alerts = r.Value;
        }

        string[] rewards = alerts
            .Where(x => x.active)
            .Select(x => $"{x.mission.faction} / {x.mission.type} [{x.mission.reward.asString}] ")
            .ToArray();

        ab.Append($"{alerts.Length} {"Alert".PluralizeOn(alerts.Length)} ➜ ")
            .Append(string.Join(" ● ", rewards));

        MessageHandler.SendColoredMessage(channel, $"@{user}, {ab}", ChatColor.Coral);
        ObjectCache.Put("alerts_wf", alerts, 150);
    }
}
