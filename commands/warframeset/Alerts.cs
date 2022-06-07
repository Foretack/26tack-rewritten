using System.Text;
using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.json;
using _26tack_rewritten.models;
using _26tack_rewritten.utils;

namespace _26tack_rewritten.commands.warframeset;
internal class Alerts : DataCacher<Alert[]>, IChatCommand
{
    public Command Info()
    {
        string name = "alerts";
        string description = "Get the current alerts in the system";
        int[] cooldowns = { 30, 5 };

        return new Command(name, description, cooldowns: cooldowns);
    }

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        StringBuilder ab = new StringBuilder();
        Alert[]? alerts;

        alerts = GetCachedPiece("alerts")?.Object;
        if (alerts is null)
        {
            alerts = await ExternalAPIHandler.GetAlerts();
            if (alerts is null)
            {
                MessageHandler.SendMessage(channel, $"@{user}, There was an error retrieving alert data PoroSad");
                return;
            }
            CachePiece("alerts", alerts, 150);
        }
        string[] rewards = alerts
            .Where(x => x.active)
            .Select(x => $"{x.mission.faction} / {x.mission.type} [{x.mission.reward.asString}] ")
            .ToArray();

        ab.Append($"{alerts.Length} Alert(s) 🡺 ")
            .Append(string.Join(" -- ", rewards));

        MessageHandler.SendColoredMessage(channel, $"@{user}, {ab}", ChatColor.Coral);
    }
}
