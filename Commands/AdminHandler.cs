using Tack.Commands.AdminSet;
using Tack.Interfaces;
using Tack.Models;

namespace Tack.Commands;
internal class AdminHandler : ChatCommandHandler
{
    public AdminHandler()
    {
        Name = "Admin";
        Prefix = "func_";
        UseUnifiedCooldowns = true;
        Cooldowns = new int[] { 0, 0 };
        Visibility = PermissionLevels.Whitelisted;

        AddCommand(new Massping());
        AddCommand(new JoinChannel());
        AddCommand(new PartChannel());
        AddCommand(new Debug());
        AddCommand(new Switch());
        AddCommand(new Exit());
    }
}
