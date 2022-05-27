using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.models;
using _26tack_rewritten.json;
using System.Text.Json;
using _26tack_rewritten.utils;

namespace _26tack_rewritten.commands.baseset;
internal class RandomJoke : IChatCommand
{
    private readonly HttpClient Requests = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(500) };
    private readonly string RequestUrl = "https://v2.jokeapi.dev/joke/Any?blacklistFlags=religious,racist&type=single";

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

        try
        {
            Stream response = await Requests.GetStreamAsync(RequestUrl);
            JokeAPI rj = (await JsonSerializer.DeserializeAsync<JokeAPI>(response))!;
            MessageHandler.SendMessage(channel, $"@{user}, [{rj.category}] {rj.joke.Replace('\n', ' ')} {new string[] { "LuL", "4Head", "xd", string.Empty }.Choice()}");
        }
        catch (Exception ex)
        {
            if (ex is TaskCanceledException)
            {
                MessageHandler.SendMessage(channel, $"@{user}, FeelsDankMan request timed out, try again maybe?");
                return;
            }
            MessageHandler.SendMessage(channel, $"@{user}, unexpected error occured :(");
        }
    }
}
