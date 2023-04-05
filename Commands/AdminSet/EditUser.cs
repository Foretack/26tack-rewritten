using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Commands.AdminSet;
internal sealed class EditUser : Command
{
    public override CommandInfo Info { get; } = new(
        name: "edituser",
        aliases: new string[] { "user", "whitelist", "blacklist" },
        permission: PermissionLevels.Whitelisted
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.Message.Author.DisplayName;
        string channel = ctx.Message.Channel.Name;
        string commandName = ctx.CommandName;
        string[] args = ctx.Args;
        if (args.Length == 0)
        {
            await MessageHandler.SendMessage(channel, $"@{user}, You need to specify a user.");
            return;
        }

        Result<User> targetResult = await User.Get(args[0]);
        if (!targetResult.Success)
        {
            await MessageHandler.SendMessage(channel, $"@{user}, That user's Twitch account was not found!");
            return;
        }

        User target = targetResult.Value;

        int mode = commandName switch
        {
            "whitelist" => 1,
            "blacklist" => 2,
            _ => 0
        };
        if (mode == 0 && (args.Length < 2 || (args[1] != "blacklist" && args[1] != "whitelist")))
        {
            await MessageHandler.SendMessage(channel, $"@{user}, Incorrect usage. {commandName} <user> <whitelist/blacklist>");
            return;
        }

        if (mode == 0 && args[1] == "whitelist")
            mode = 1;
        if (mode == 0 && args[1] == "blacklist")
            mode = 2;

        if (mode == 1)
            await WhitelistUser(target.Username);
        else
            await BlacklistUser(target.Username, target.Id);

        await MessageHandler.SendMessage(channel, "k");
    }

    private async Task BlacklistUser(string username, long id)
    {
        DbQueries db = SingleOf<DbQueries>.Obj;
        await db.BlacklistUser(username, id);
        Permission.BlacklistUser(username);
    }
    private async Task WhitelistUser(string username)
    {
        DbQueries db = SingleOf<DbQueries>.Obj;
        await db.WhitelistUser(username);
        Permission.WhitelistUser(username);
    }
}
