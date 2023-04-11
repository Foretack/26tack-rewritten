using System.Text;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;
using TwitchLib.Api.Helix.Models.Chat;

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
        string user = ctx.Message.Author.DisplayName;
        string channel = ctx.Message.Channel.Name;
        var perms = (PermissionLevels)ctx.Permission.Level;

        StringOperator op = new();
        string prefix = ctx.CommandName;
        bool s = CommandHandler.Handlers.TryGetValue(prefix, out ChatCommandHandler? handler);
        if (!s || handler is null)
        {
            await MessageHandler.SendMessage(channel, $"Something went wrong internally. Try again later?");
            Log.Error("commands command failed to get the handler for a prefix somehow pajaS");
            return;
        }

        await Task.Run(async () =>
        {
            _ = op
            % $"@{user} "
            % " commands: ";

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

            _ = op % list.AsString();

            await MessageHandler.SendColoredMessage(channel, op.ToString(), UserColors.SeaGreen);
        });
    }
}
