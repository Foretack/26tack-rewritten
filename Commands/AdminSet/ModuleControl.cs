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

    public override Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.Username;
        string channel = ctx.IrcMessage.Channel;

        if (ctx.Args.Length < 2)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Usage: <ModuleName> <enable/disable>");
            return Task.CompletedTask;
        }

        string moduleName = ctx.Args[0];
        string action = ctx.Args[1].ToLower();

        if (action is not "enable" and not "disable")
        {
            MessageHandler.SendMessage(channel, $"@{user}, Unrecognized action: {action}");
            return Task.CompletedTask;
        }

        if (action == "enable")
        {
            bool enabled = ModulesHandler.EnableModule(moduleName);
            MessageHandler.SendMessage(channel, $"{(enabled ? "Enabled" : "Error enabling")} {moduleName}");
            return Task.CompletedTask;
        }

        bool disabled = ModulesHandler.DisableModule(moduleName);
        MessageHandler.SendMessage(channel, $"{(disabled ? "Disabled" : "Error disabling")} {moduleName}");
        return Task.CompletedTask;
    }
}