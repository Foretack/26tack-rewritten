using Tack.Commands.WarframeSet;
using Tack.Nonclass;

namespace Tack.Commands;
internal class WarframeHandler : ChatCommandHandler
{
    internal const string BaseUrl = "https://api.warframestat.us/pc";

    public WarframeHandler()
    {
        Name = "Warframe";
        Prefix = "wf>";

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
    }
}
