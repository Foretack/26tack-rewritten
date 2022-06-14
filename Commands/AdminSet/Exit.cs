using Tack.Handlers;
using Tack.Interfaces;
using Tack.Models;

namespace Tack.Commands.AdminSet;
internal class Exit : IChatCommand
{
    public Command Info()
    {
        string name = "exit";
        string[] aliases = { "quit" };
        PermissionLevels permission = PermissionLevels.Whitelisted;

        return new Command(name, aliases: aliases, permission: permission);
    }

    public Task Run(CommandContext ctx)
    {
        Environment.Exit(0);
        return Task.CompletedTask;
    }
}
