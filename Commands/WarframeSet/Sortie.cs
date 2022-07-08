using Tack.Handlers;
using Tack.Interfaces;
using Tack.Json;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal class Sortie : IChatCommand
{
    public Command Info()
    {
        string name = "sortie";
        string description = "Check the current Sortie";
        string[] aliases = { "anasa", "sorties" };
        int[] cooldowns = { 5, 3 };

        return new Command(name, description, aliases, cooldowns);
    }

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        CurrentSortie? sortie = ObjectCache.Get<CurrentSortie>("current_sortie_wf")
            ?? await ExternalAPIHandler.GetSortie();
        if (sortie is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Failed to fetch the current sortie. Try again later?");
            return;
        }

        TimeSpan timeLeft = sortie.expiry.ToLocalTime() - DateTime.Now.ToLocalTime();
        if (timeLeft.TotalSeconds < 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Sortie data is outdated. You should try again later ppL");
            return;
        }
        string sortieString = $"{sortie.faction} -- " +
            $"● {sortie.variants[0].missionType} [{sortie.variants[0].modifier}] " +
            $"●● {sortie.variants[1].missionType} [{sortie.variants[1].modifier}] " +
            $"●●● {(sortie.variants[2].missionType == "Assassination" ? $"{sortie.boss} Assassination" : sortie.variants[2].missionType)} [{sortie.variants[2].modifier}]";

        MessageHandler.SendMessage(channel, $"@{user}, {sortieString} 🡺 time left: {timeLeft.FormatTimeLeft()}");
        ObjectCache.Put("current_sortie_wf", sortie, (int)timeLeft.TotalSeconds);
    }
}
