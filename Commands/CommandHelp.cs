using System.Text;
using Tack.Handlers;
using Tack.Nonclass;
using Tack.Models;
using Serilog;

namespace Tack.Commands;
internal static class CommandHelp
{
    public static Command Info()
    {
        string name = "help";
        string description = "Get information on a command's usage, aliases, cooldowns and permission";
        int[] cooldowns = { 3, 0 };
        PermissionLevels permission = PermissionLevels.EveryonePlusBlacklisted;

        return new Command(name, description, cooldowns: cooldowns, permission: permission);
    }

    public static async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        string prefix = ctx.CommandName;

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, you need to specify which command dummy ({prefix}commands for a list)");
            return;
        }
        bool s = CommandHandler.Handlers.TryGetValue(prefix, out ChatCommandHandler? handler);
        if (!s || handler is null)
        {
            MessageHandler.SendMessage(channel, $"Something went wrong internally. Try again later?");
            Log.Error($"help command failed to get the handler for a prefix somehow pajaS");
            return;
        }

        await Task.Run(() =>
        {
            IChatCommand? command = null;

            try
            {
                command = handler.Commands.First(x => x.Key.Contains(args[0])).Value;
            }
            catch (InvalidOperationException)
            {
                MessageHandler.SendMessage(channel, $"@{user}, No command with that name or aliases was found. Use {prefix}commands to see commands FeelsDankMan");
                return;
            }

            Command cmdinfo = command.Info();
            StringBuilder sb = new StringBuilder($"@{user}, ");

            sb.Append($"Command: {prefix}{cmdinfo.Name}")
            .Append(" -- ")
            .Append($"Aliases: [{string.Join(", ", cmdinfo.Aliases)}]")
            .Append(" -- ")
            .Append($"Permission: {cmdinfo.Permission}")
            .Append(" 🡺 ")
            .Append(cmdinfo.Description)
            .Append(" 🡺 ")
            .Append($"{cmdinfo.Cooldowns[0]}s user cooldown, ")
            .Append($"{cmdinfo.Cooldowns[1]}s channel cooldown.");

            MessageHandler.SendMessage(channel, sb.ToString());
        });
    }
}
