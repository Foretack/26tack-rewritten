using System.Text;
using Tack.Handlers;
using Tack.Interfaces;
using Tack.Json;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal class Alerts : IChatCommand
{
    public Command Info()
    {
        string name = "alerts";
        string description = "Get the current alerts in the system";
        int[] cooldowns = { 10, 3 };

        return new Command(name, description, cooldowns: cooldowns);
    }

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        StringBuilder ab = new StringBuilder();

        Alert[]? alerts = ObjectCaching.GetCachedObject<Alert[]>("alerts_wf")
            ?? await ExternalAPIHandler.GetAlerts();
        if (alerts is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, There was an error retrieving alert data PoroSad");
            return;
        }

        string[] rewards = alerts
            .Where(x => x.active)
            .Select(x => $"{x.mission.faction} / {x.mission.type} [{x.mission.reward.asString}] ")
            .ToArray();

        ab.Append($"{alerts.Length} Alert(s) 🡺 ")
            .Append(string.Join(" -- ", rewards));

        MessageHandler.SendColoredMessage(channel, $"@{user}, {ab}", ChatColor.Coral);
        ObjectCaching.CacheObject("alerts_wf", alerts, 150);
    }
}
