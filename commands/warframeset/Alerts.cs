using System.Text;
using System.Text.Json;
using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.json;
using _26tack_rewritten.models;
using _26tack_rewritten.utils;
using Serilog;

namespace _26tack_rewritten.commands.warframeset;
internal class Alerts : DataCacher<Alert[]>, IChatCommand
{
    private static readonly HttpClient Requests = new HttpClient() { Timeout = TimeSpan.FromSeconds(1) };

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
            Stream aResponse = await Requests.GetStreamAsync(WarframeHandler.BaseUrl + "/alerts");
            alerts = await JsonSerializer.DeserializeAsync<Alert[]>(aResponse);
            if (alerts is null)
            {
                Log.Error("Serialization of current alerts failed");
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
