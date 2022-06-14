using System.Text;
using Tack.Handlers;
using Tack.Interfaces;
using Tack.Models;
using Serilog;

namespace Tack.Commands;
internal static class CommandList
{
    public  static Command Info()
    {
        string name = "commands";
        string description = "Returns a list of all available commands & command sets";
        string[] aliases = { "cmds", "cmdlist", "commandlist", "commands" };
        int[] cooldowns = { 3, 0 };
        PermissionLevels permission = PermissionLevels.EveryonePlusBlacklisted;

        return new Command(name, description, aliases, cooldowns, permission);
    }

    public static async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        PermissionLevels perms = (PermissionLevels)ctx.Permission.Level;
        
        StringBuilder sb = new StringBuilder($"@{user} ");
        string prefix = ctx.CommandName;
        bool s = CommandHandler.Handlers.TryGetValue(prefix, out ChatCommandHandler? handler);
        if (!s || handler is null)
        {
            MessageHandler.SendMessage(channel, $"Something went wrong internally. Try again later?");
            Log.Error($"commands command failed to get the handler for a prefix somehow pajaS");
            return;
        }

        await Task.Run(() => {
            sb.Append(handler.Name + " commands: ");
            sb.Append('[');

            var commandNames = handler.Commands
            .Select(x => prefix + x.Value.Info().Name)
            .AsEnumerable();
            List<string> list = new List<string>(commandNames);
            list.Add(prefix + "help");
            var otherSets = CommandHandler.Handlers
            .Where(x => x.Key != prefix && x.Value.Visibility <= perms)
            .Select(y => y.Key + "commands").AsEnumerable();
            list.AddRange(otherSets);

            sb.Append(string.Join(" | ", list));
            sb.Append(']');

            MessageHandler.SendColoredMessage(channel, sb.ToString(), ChatColor.Green);
        });
    }
}
