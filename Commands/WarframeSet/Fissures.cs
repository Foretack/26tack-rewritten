using Tack.Handlers;
using Tack.Json;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal sealed class Fissures : Command
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

        var r = await ExternalAPIHandler.WarframeStatusApi<Fissure[]>("fissures");
        if (!r.Success)
        {
            MessageHandler.SendMessage(channel, $"@{user}, ⚠ Http error! {r.Exception.Message}");
            return;
        }

        var fissures = r.Value;
        fissures = fissures.Where(x => x.Active).ToArray();

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
        int lCount = fissures.Count(x => includeStorms ? x.TierNum == 1 : x.TierNum == 1 && !x.IsStorm);
        int mCount = fissures.Count(x => includeStorms ? x.TierNum == 2 : x.TierNum == 2 && !x.IsStorm);
        int nCount = fissures.Count(x => includeStorms ? x.TierNum == 3 : x.TierNum == 3 && !x.IsStorm);
        int aCount = fissures.Count(x => includeStorms ? x.TierNum == 4 : x.TierNum == 4 && !x.IsStorm);
        int rCount = fissures.Count(x => includeStorms ? x.TierNum == 5 : x.TierNum == 5 && !x.IsStorm);

        return $"{lCount} Lith, {mCount} Meso, {nCount} Neo, {aCount} Axi, {rCount} Requiem 🥜";
    }

    private string ListFissureMissions(Fissure[] fissures, int tierNum, bool includeStorms)
    {
        string[] missions = fissures
            .Where(x => includeStorms ? x.TierNum == tierNum : x.TierNum == tierNum && !x.IsStorm)
            .Select(m => $"{m.Enemy} {m.MissionKey} ({m.Eta})")
            .ToArray();
        string mString = string.Join(" ◯ ", missions) + " 🥜";
        return mString;
    }
}
