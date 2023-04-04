using Tack.Core;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Commands.AdminSet;
internal sealed class Update : Command
{
    public override CommandInfo Info { get; } = new(
        name: "update",
        description: "yep!! this is the update command!! xD",
        permission: PermissionLevels.Whitelisted
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.Message.Author.DisplayName;
        string channel = ctx.Message.Channel.Name;

        DbQueries db = SingleOf<DbQueries>.Obj;
        _ = await db.LogException(new Debug.TestException($"UPDATE COMMAND USAGE BY {user}"));

        string? pullResult = await Program.GitPull();

        if (pullResult is null)
        {
            await MessageHandler.SendMessage(channel, $"something went wrong CaitlynS");
            return;
        }

        if (pullResult.Contains("Already up to date."))
        {
            await MessageHandler.SendMessage(channel, $"Pepega {pullResult}");
            return;
        }

        await MessageHandler.SendMessage(channel, $"PPardner 🔧 {user} {pullResult}");
    }
}
