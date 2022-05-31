using _26tack_rewritten.commands.adminset;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.models;

namespace _26tack_rewritten.commands;
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
    }
}
