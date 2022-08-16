using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Commands.AdminSet;
internal class Exit : Command
{
    public override CommandInfo Info { get; } = new(
        name: "exit",
        aliases: new string[] { "quit" },
        permission: PermissionLevels.Whitelisted
    );

    public override Task Execute(CommandContext ctx)
    {
        Environment.Exit(0);
        return Task.CompletedTask;
    }
}
