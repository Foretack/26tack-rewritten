using System.Linq;
using System.Text;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;
using TwitchLib.Api.Helix.Models.Chat;

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
        string user = ctx.Message.Author.DisplayName;
        string channel = ctx.Message.Channel.Name;
        StringOperator ab = new();

        (bool keyExists, Alert[] value) = await Redis.Cache.TryGetObjectAsync<Alert[]>("warframe:alerts");
        if (!keyExists)
        {
            Result<Alert[]> r = await ExternalApiHandler.WarframeStatusApi<Alert[]>("alerts");
            if (!r.Success)
            {
                await MessageHandler.SendMessage(channel, $"@{user}, ⚠ Request failed: {r.Exception.Message}");
                return;
            }

            await Redis.Cache.SetObjectAsync("warframe:alerts", r.Value, TimeSpan.FromMinutes(2.5));
            value = r.Value;
        }

        Alert[] alerts = value;
        string[] rewards = alerts
            .Where(x => x.Active)
            .Select(x => $"{x.Mission.Faction} / {x.Mission.Type} [{x.Mission.Reward.AsString}] ")
            .ToArray();

        _ = ab % $"{"Alert".PluralizeWith(alerts.Length)} ➜ " % string.Join(" ● ", rewards);
        await MessageHandler.SendColoredMessage(channel, $"@{user}, {ab}", UserColors.Coral);
    }
}
