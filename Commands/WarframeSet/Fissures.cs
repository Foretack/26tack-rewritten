using Tack.Handlers;
using Tack.Nonclass;
using Tack.Json;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal class Fissures : Command
{
    public override CommandInfo Info { get; } = new(
        name: "fissures", 
        description: "Get information about active void fissures. Additional options: `storms:true/false` (true default)", 
        aliases: new string[] { "fissure", "f" }
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        string fissuresString;

        Fissure[]? fissures = (await ExternalAPIHandler.WarframeStatusApi<Fissure[]>("fissures")).Value;
        if (fissures is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, There was an error retrieving fissure data PoroSad");
            return;
        }

        fissures = fissures.Where(x => x.active).ToArray();

        if (args.Length == 0)
        {
            fissuresString = ListAllFissures(fissures, true);
            MessageHandler.SendColoredMessage(channel, $"@{user}, {fissuresString}", ChatColor.Coral);
            return;
        }
        bool includeStorms = Options.ParseBool("storms", ctx.IrcMessage.Message) ?? true;

        fissuresString = args[0].ToLower() switch
        {
            "lith" => ListFissureMissions(fissures, 1, includeStorms),
            "meso" => ListFissureMissions(fissures, 2, includeStorms),
            "neo" => ListFissureMissions(fissures, 3, includeStorms),
            "axi" => ListFissureMissions(fissures, 4, includeStorms),
            "requiem" => ListFissureMissions(fissures, 5, includeStorms),
            _ => ListAllFissures(fissures, includeStorms)
        };
        MessageHandler.SendColoredMessage(channel, $"@{user}, {fissuresString}", ChatColor.Coral);
    }

    private string ListAllFissures(Fissure[] fissures, bool includeStorms)
    {
        int lCount = fissures.Count(x => includeStorms ? x.tierNum == 1 : x.tierNum == 1 && !x.isStorm);
        int mCount = fissures.Count(x => includeStorms ? x.tierNum == 2 : x.tierNum == 2 && !x.isStorm);
        int nCount = fissures.Count(x => includeStorms ? x.tierNum == 3 : x.tierNum == 3 && !x.isStorm);
        int aCount = fissures.Count(x => includeStorms ? x.tierNum == 4 : x.tierNum == 4 && !x.isStorm);
        int rCount = fissures.Count(x => includeStorms ? x.tierNum == 5 : x.tierNum == 5 && !x.isStorm);

        return $"{lCount} Lith, {mCount} Meso, {nCount} Neo, {aCount} Axi, {rCount} Requiem 🥜";
    }

    private string ListFissureMissions(Fissure[] fissures, int tierNum, bool includeStorms)
    {
        string[] missions = fissures
            .Where(x => includeStorms ? x.tierNum == tierNum : x.tierNum == tierNum && !x.isStorm)
            .Select(m => $"{m.enemy} {m.missionType} ({m.eta})")
            .ToArray();
        string mString = string.Join(" ◯ ", missions) + " 🥜";
        return mString;
    }
}
