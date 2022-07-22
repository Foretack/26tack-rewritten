using Tack.Handlers;
using Tack.Nonclass;
using Tack.Models;
using C = Tack.Core.Core;
using Tack.Database;

namespace Tack.Commands.AdminSet;
internal class Update : Command
{
    public override CommandInfo Info { get; } = new(
        name: "update",
        description: "yep!! this is the update command!! xD",
        permission: PermissionLevels.Whitelisted
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        DbQueries db = new DbQueries();
        await db.LogException(new Debug.TestException($"UPDATE COMMAND USAGE BY {user}"));

        string? pullResult = await C.GitPull();

        if (pullResult is null)
        {
            MessageHandler.SendMessage(channel, $"something went wrong CaitlynS");
            return;
        }

        if (pullResult.Contains("Already up to date."))
        {
            MessageHandler.SendMessage(channel, $"Pepega {pullResult}");
            return;
        }

        MessageHandler.SendMessage(channel, $"PPardner 🔧 {user} {pullResult}");
    }
}
