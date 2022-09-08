using Tack.Commands.AdminSet;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Commands;
internal class AdminHandler : ChatCommandHandler
{
    public override string Name => "Admin";
    public override string Prefix => "f_";
    public override bool UseUnifiedCooldowns => true;
    public override short UserCooldown => 0;
    public override short ChannelCooldown => 0;
    public override PermissionLevels Visibility => PermissionLevels.Whitelisted;

    public AdminHandler()
    {
        AddCommand(new Massping());
        AddCommand(new JoinChannel());
        AddCommand(new PartChannel());
        AddCommand(new Debug());
        AddCommand(new Switch());
        AddCommand(new Exit());
        AddCommand(new Update());
        AddCommand(new EditUser());
        AddCommand(new ModuleControl());
    }
}
