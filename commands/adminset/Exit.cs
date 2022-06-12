using _26tack_rewritten.core;
using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.models;

namespace _26tack_rewritten.commands.adminset;
internal class Exit : IChatCommand
{
    public Command Info()
    {
        string name = "exit";
        string[] aliases = { "quit" };
        PermissionLevels permission = PermissionLevels.Whitelisted;

        return new Command(name, aliases: aliases, permission: permission);
    }

    public async Task Run(CommandContext ctx)
    {
        MainClient.Running = false;
    }
}
