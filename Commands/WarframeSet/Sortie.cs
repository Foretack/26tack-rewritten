using Tack.Handlers;
using Tack.Nonclass;
using Tack.Json;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal class Sortie : IChatCommand
{
    public Command Info() => new(
        name: "sortie",
        description: "Check the current Sortie",
        aliases: new string[] { "anasa", "sorties" },
        cooldowns: new int[] { 5, 3 }
        );

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        CurrentSortie? sortie = ObjectCache.Get<CurrentSortie>("current_sortie_wf");
        if (sortie is null)
        {
            var r = await ExternalAPIHandler.WarframeStatusApi<CurrentSortie>("sortie");
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, Failed to fetch the current sortie. ({r.Exception.Message})");
                return; 
            }
            sortie = r.Value;
        }

        TimeSpan timeLeft = sortie.expiry.ToLocalTime() - DateTime.Now.ToLocalTime();
        if (timeLeft.TotalSeconds < 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Sortie data is outdated. You should try again later ppL");
            return;
        }
        string sortieString = $"{sortie.faction} -- " +
            $"● {sortie.variants[0].missionType} [{sortie.variants[0].modifier}] " +
            $"■ {sortie.variants[1].missionType} [{sortie.variants[1].modifier}] " +
            $"◆ {(sortie.variants[2].missionType == "Assassination" ? $"{sortie.boss} Assassination" : sortie.variants[2].missionType)} [{sortie.variants[2].modifier}]";

        MessageHandler.SendMessage(channel, $"@{user}, {sortieString} -- time left: {timeLeft.FormatTimeLeft()}");
        ObjectCache.Put("current_sortie_wf", sortie, (int)timeLeft.TotalSeconds);
    }
}
