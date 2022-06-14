using Tack.Handlers;
using Tack.Interfaces;
using Tack.Models;
using Tack.Database;

namespace Tack.Commands.BaseSet;
internal class Suggest : IChatCommand
{
    public Command Info()
    {
        string name = "suggest";
        string description = "Suggest a feature / give feedback for the bot";
        int[] cooldowns = { 600, 5 };

        return new Command(name, description, cooldowns: cooldowns);
    }

    public async Task Run(CommandContext ctx)
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

        Database.Database db = new Database.Database();
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
