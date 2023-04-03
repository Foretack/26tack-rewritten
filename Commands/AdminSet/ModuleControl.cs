using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Commands.AdminSet;
internal sealed class ModuleControl : Command
{
    public override CommandInfo Info { get; } = new(
        name: "module",
        description: "Used to control chat modules",
        permission: PermissionLevels.Whitelisted
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.Message.Author.Name;
        string channel = ctx.Message.Channel.Name;

        if (ctx.Args.Length < 2)
        {
            await MessageHandler.SendMessage(channel, $"@{user}, Usage: <ModuleName> <enable/disable>");
            return;
        }

        string moduleName = ctx.Args[0];
        string action = ctx.Args[1].ToLower();

        if (action is not "enable" and not "disable")
        {
            await MessageHandler.SendMessage(channel, $"@{user}, Unrecognized action: {action}");
            return;
        }

        if (action == "enable")
        {
            bool enabled = ModulesHandler.EnableModule(moduleName);
            await MessageHandler.SendMessage(channel, $"{(enabled ? "Enabled" : "Error enabling")} {moduleName}");
            return;
        }

        bool disabled = ModulesHandler.DisableModule(moduleName);
        await MessageHandler.SendMessage(channel, $"{(disabled ? "Disabled" : "Error disabling")} {moduleName}");
        return;
    }
}
