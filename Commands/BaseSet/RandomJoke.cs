using Tack.Handlers;
using Tack.Json;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.BaseSet;
internal sealed class RandomJoke : Command
{
    public override CommandInfo Info { get; } = new(
        name: "randomjoke",
        description: "Get a random joke LuL ",
        aliases: new string[] { "4head", "rj" }
    );

    private readonly string[] _appendedEndings = new string[] { "LuL", "4Head", "xd", string.Empty };

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        var result = await ExternalAPIHandler.GetInto<JokeAPI>("https://v2.jokeapi.dev/joke/Any?blacklistFlags=religious,racist&type=single");

        if (!result.Success)
        {
            MessageHandler.SendMessage(channel, $"@{user}, there was an error retrieving a random joke :( -> {result.Exception.Message}");
            return;
        }
        var joke = result.Value;
        MessageHandler.SendMessage(channel, $"@{user}, [{joke.Category}] {joke.Joke.Replace('\n', ' ')} {_appendedEndings.Choice()}");
    }
}
