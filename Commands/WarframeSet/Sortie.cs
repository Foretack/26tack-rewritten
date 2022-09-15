using Tack.Handlers;
using Tack.Json;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal sealed class Sortie : Command
{
    public override CommandInfo Info { get; } = new(
        name: "sortie",
        description: "Check the current Sortie",
        aliases: new string[] { "anasa", "sorties" },
        userCooldown: 5,
        channelCooldown: 3
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        CurrentSortie? sortie = ObjectCache.Get<CurrentSortie>("current_sortie_wf");
        if (sortie is null)
        {
            Result<CurrentSortie> r = await ExternalAPIHandler.WarframeStatusApi<CurrentSortie>("sortie");
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, Failed to fetch the current sortie. ({r.Exception.Message})");
                return;
            }
            sortie = r.Value;
        }

        TimeSpan timeLeft = sortie.Expiry.ToLocalTime() - DateTime.Now.ToLocalTime();
        if (timeLeft.TotalSeconds < 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Sortie data is outdated. You should try again later ppL");
            return;
        }
        string sortieString = $"{sortie.Faction} " +
            $"➜ ● {sortie.Variants[0].MissionType} [{sortie.Variants[0].Modifier}] " +
            $"➜ ■ {sortie.Variants[1].MissionType} [{sortie.Variants[1].Modifier}] " +
            $"➜ ◆ {(sortie.Variants[2].MissionType == "Assassination" ? $"{sortie.Boss} Assassination" : sortie.Variants[2].MissionType)} [{sortie.Variants[2].Modifier}]";

        MessageHandler.SendMessage(channel, $"@{user}, {sortieString} -- time left: {timeLeft.FormatTimeLeft()}");
        ObjectCache.Put("current_sortie_wf", sortie, (int)timeLeft.TotalSeconds);
    }
}
