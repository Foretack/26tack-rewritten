using _26tack_rewritten.commands.warframeset;
using _26tack_rewritten.interfaces;

namespace _26tack_rewritten.commands;
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
    }
}
