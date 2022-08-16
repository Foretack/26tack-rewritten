using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Commands.AdminSet;
internal class EditUser : Command
{
    public override CommandInfo Info { get; } = new(
        name: "edituser",
        aliases: new string[] { "user", "whitelist", "blacklist" },
        permission: PermissionLevels.Whitelisted
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string commandName = ctx.CommandName;
        string[] args = ctx.Args;
        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, You need to specify a user.");
            return;
        }

        var uf = new UserFactory();
        User? target = await uf.CreateUserAsync(args[0]);
        if (target is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, That user's Twitch account was not found!");
            return;
        }

        int mode = commandName switch
        {
            "whitelist" => 1,
            "blacklist" => 2,
            _ => 0
        };
        if (mode == 0 && (args.Length < 2 || (args[1] != "blacklist" && args[1] != "whitelist")))
        {
            MessageHandler.SendMessage(channel, $"@{user}, Incorrect usage. {commandName} <user> <whitelist/blacklist>");
            return;
        }
        if (mode == 0 && args[1] == "whitelist") mode = 1;
        if (mode == 0 && args[1] == "blacklist") mode = 2;

        bool success = mode == 1 ? await WhitelistUser(target.Username) : await BlacklistUser(target.Username, target.ID);
        MessageHandler.SendMessage(channel, success.ToString());
    }

    private async Task<bool> BlacklistUser(string username, string id)
    {
        var db = new DbQueries();
        bool s = await db.BlacklistUser(username, id);
        Permission.BlacklistUser(username);
        return s;
    }
    private async Task<bool> WhitelistUser(string username)
    {
        var db = new DbQueries();
        bool s = await db.WhitelistUser(username);
        Permission.WhitelistUser(username);
        return s;
    }
}
