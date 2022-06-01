using System.Text;
using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.models;

namespace _26tack_rewritten.commands;
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
        string[] args = ctx.Args;
        PermissionLevels perms = (PermissionLevels)ctx.Permission.Level;
        
        StringBuilder sb = new StringBuilder($"@{user} ");
        string prefix = ctx.CommandName;
        bool s = CommandHandler.Handlers.TryGetValue(prefix, out ChatCommandHandler? handler);
        if (!s || handler is null) return;

        await Task.Run(() => {
            sb.Append(handler.Name + " commands: ");
            sb.Append('[');

            var commandNames = handler.Commands
            .Select(x => prefix + x.Value.Info().Name)
            .AsEnumerable();
            List<string> list = new List<string>(commandNames);
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
