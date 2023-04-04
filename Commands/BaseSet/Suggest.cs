using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using TwitchLib.Api.Helix.Models.Chat;

namespace Tack.Commands.BaseSet;
internal sealed class Suggest : Command
{
    public override CommandInfo Info { get; } = new(
        name: "suggest",
        description: "Suggest a feature / give feedback for the bot",
        userCooldown: 600
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.Message.Author.DisplayName;
        string rawname = ctx.Message.Author.Name;
        long id = ctx.Message.Author.Id;
        string channel = ctx.Message.Channel.Name;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            await MessageHandler.SendMessage(channel, $"@{user}, FeelsDankMan you must give a suggestion to... suggest something... ");
            return;
        }

        DbQueries db = SingleOf<DbQueries>.Obj;
        var partialUser = new PartialUser(user, rawname, id);
        bool success = await db.CreateSuggestion(partialUser, string.Join(' ', args).Replace('\'', '_'));

        if (success)
        {
            await MessageHandler.SendColoredMessage(channel, $"@{user}, ApuApustaja 👍 Your suggestion has been saved. You will most likely be " +
                $"notified through a supibot reminder regarding it's status", UserColors.SeaGreen);
            return;
        }

        await MessageHandler.SendMessage(channel, $"@{user}, PoroSad There was an error processing your suggestion. Try again later?");
    }
}
