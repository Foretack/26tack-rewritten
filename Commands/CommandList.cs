using System.Text;
using Serilog;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands;
internal static class CommandList
{
    public static CommandInfo Info { get; } = new(
        name: "commands",
        description: "Returns a list of all available commands & command sets",
        aliases: new string[] { "cmds", "cmdlist", "commandlist", "commands" },
        userCooldown: 3,
        channelCooldown: 0,
        permission: PermissionLevels.EveryonePlusBlacklisted
    );

    public static async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        var perms = (PermissionLevels)ctx.Permission.Level;

        var sb = new StringBuilder($"@{user} ");
        string prefix = ctx.CommandName;
        bool s = CommandHandler.Handlers.TryGetValue(prefix, out ChatCommandHandler? handler);
        if (!s || handler is null)
        {
            MessageHandler.SendMessage(channel, $"Something went wrong internally. Try again later?");
            Log.Error($"commands command failed to get the handler for a prefix somehow pajaS");
            return;
        }

        await Task.Run(() =>
        {
            _ = sb.Append(handler.Name + " commands: ");

            IEnumerable<string> commandNames = handler.Commands
            .Select(x => prefix + x.Value.Info.Name)
            .AsEnumerable();
            var list = new List<string>(commandNames)
            {
                prefix + "help"
            };
            IEnumerable<string> otherSets = CommandHandler.Handlers
            .Where(x => x.Key != prefix && x.Value.Visibility <= perms)
            .Select(y => y.Key + "commands").AsEnumerable();
            list.AddRange(otherSets);

            _ = sb.Append(list.AsString());

            MessageHandler.SendColoredMessage(channel, sb.ToString(), ChatColor.Green);
        });
    }
}
