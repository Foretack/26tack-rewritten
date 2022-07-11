using Tack.Handlers;
using Tack.Interfaces;
using Tack.Models;

namespace Tack.Commands.AdminSet;
internal class Exit : IChatCommand
{
    public Command Info() => new(
        name: "exit",
        aliases: new string[] {"quit"},
        permission: PermissionLevels.Whitelisted
        );

    public Task Run(CommandContext ctx)
    {
        Environment.Exit(0);
        return Task.CompletedTask;
    }
}
