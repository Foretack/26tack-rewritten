using Tack.Commands.WarframeSet;
using Tack.Nonclass;

namespace Tack.Commands;
internal sealed class WarframeHandler : ChatCommandHandler
{
    public override string Name => "Warframe";
    public override string Prefix => "wf>";
    internal const string BaseUrl = "https://api.warframestat.us/pc";

    public WarframeHandler()
    {
        AddCommand(new Alerts());
        AddCommand(new Fissures());
        AddCommand(new Sortie());
        AddCommand(new Cycle());
        AddCommand(new Market());
        AddCommand(new Relics());
        AddCommand(new Invasions());
        AddCommand(new SteelPath());
        AddCommand(new Drops());
        AddCommand(new Mods());
        AddCommand(new Profile());
        AddCommand(new ArchonHunt());
    }
}
