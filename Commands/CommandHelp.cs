using System.Text;
using Tack.Handlers;
using Tack.Nonclass;
using Tack.Models;
using Serilog;
using Tack.Utils;

namespace Tack.Commands;
internal static class CommandHelp
{
    public static CommandInfo Info { get; } = new(
        name: "help",
        description: "Get information on a command's usage, aliases, cooldowns and permission",
        aliases: new string[] { "help" },
        userCooldown: 3,
        channelCooldown: 0,
        permission: PermissionLevels.EveryonePlusBlacklisted
    );

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
            Command? command = null;

            try
            {
                command = handler.Commands.First(x => x.Key.Contains(args[0])).Value;
            }
            catch (InvalidOperationException)
            {
                MessageHandler.SendMessage(channel, $"@{user}, No command with that name or aliases was found. Use {prefix}commands to see commands FeelsDankMan");
                return;
            }

            CommandInfo cmdinfo = command.Info;
            StringBuilder sb = new StringBuilder($"@{user}, ");

            sb.Append($"Command: {prefix}{cmdinfo.Name}")
            .Append(" -- ")
            .Append($"Aliases: {cmdinfo.Aliases.AsString()}")
            .Append(" -- ")
            .Append($"Permission: {cmdinfo.Permission}")
            .Append(" ➜ ")
            .Append(cmdinfo.Description)
            .Append(" ➜ ")
            .Append($"{cmdinfo.UserCooldown}s user cooldown, ")
            .Append($"{cmdinfo.ChannelCooldown}s channel cooldown.");

            MessageHandler.SendMessage(channel, sb.ToString());
        });
    }
}
