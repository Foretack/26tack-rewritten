using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.models;
using _26tack_rewritten.utils;

namespace _26tack_rewritten.commands.baseset;
internal class RandomJoke : IChatCommand
{
    public Command Info()
    {
        string name = "randomjoke";
        string description = "Get a random joke LuL ";
        string[] aliases = { "4head", "rj" };

        return new Command(name, description, aliases);
    }

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        var rj = await ExternalAPIHandler.GetRandomJoke();

        if (rj is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, there was an error retrieving a random joke :(");
        }
        MessageHandler.SendMessage(channel, $"@{user}, [{rj.category}] {rj.joke.Replace('\n', ' ')} {new string[] { "LuL", "4Head", "xd", string.Empty }.Choice()}");
    }
}
