using Tack.Database;
using Tack.Handlers;
using Tack.Nonclass;
using Tack.Models;

namespace Tack.Commands.BaseSet;
internal class Suggest : Command
{
    public override CommandInfo Info { get; } = new(
        name: "suggest",
        description: "Suggest a feature / give feedback for the bot",
        userCooldown: 600
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string rawname = ctx.IrcMessage.Username;
        string id = ctx.IrcMessage.UserId;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, FeelsDankMan you must give a suggestion to... suggest something... ");
            return;
        }

        DbQueries db = new DbQueries();
        PartialUser partialUser = new PartialUser(user, rawname, id);
        bool success = await db.CreateSuggestion(partialUser, string.Join(' ', args).Replace('\'', '_'));

        if (success)
        {
            MessageHandler.SendColoredMessage(channel, $"@{user}, ApuApustaja 👍 Your suggestion has been saved. You will most likely be " +
                $"notified through a supibot reminder regarding it's status", ChatColor.SeaGreen);
            return;
        }
        MessageHandler.SendMessage(channel, $"@{user}, PoroSad There was an error processing your suggestion. Try again later?");
    }
}
