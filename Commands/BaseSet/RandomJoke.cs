using Tack.Handlers;
using Tack.Nonclass;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.BaseSet;
internal class RandomJoke : Command
{
    public override CommandInfo Info { get; } = new(
        name: "randomjoke",
        description: "Get a random joke LuL ",
        aliases: new string[] { "4head", "rj" }
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        var rj = await ExternalAPIHandler.GetRandomJoke();

        if (rj is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, there was an error retrieving a random joke :(");
            return;
        }
        MessageHandler.SendMessage(channel, $"@{user}, [{rj.Category}] {rj.Joke.Replace('\n', ' ')} {new string[] { "LuL", "4Head", "xd", string.Empty }.Choice()}");
    }
}
