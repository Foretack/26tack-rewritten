using Tack.Handlers;
using Tack.Interfaces;
using Tack.Models;
using C = Tack.Core.Core;
using Db = Tack.Database.Database;

namespace Tack.Commands.AdminSet;
internal class Update : IChatCommand
{
    public Command Info()
    {
        string name = "update";
        string description = "yep!! this is the update command!! xD";
        string[] aliases = { "pull" };
        PermissionLevels permission = PermissionLevels.Whitelisted;

        return new Command(name, description, aliases, permission: permission);
    }

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        Db db = new Db();
        await db.LogException(new Debug.TestException($"UPDATE COMMAND USAGE BY {user}"));

        string? pullResult = await C.GitPull();

        if (pullResult is null || !pullResult.Contains(" changed, "))
        {
            MessageHandler.SendMessage(channel, $"something went wrong CaitlynS");
            return;
        }

        if (pullResult.Contains("Already up to date."))
        {
            MessageHandler.SendMessage(channel, $"Pepega {pullResult}");
            return;
        }

        MessageHandler.SendMessage(channel, $"FeelsDankMan {user} {pullResult} | Process should restart shortly...");
        await Task.Delay(TimeSpan.FromSeconds(10));
        MessageHandler.SendMessage(channel, $"Pepege nevermind");
    }
}
