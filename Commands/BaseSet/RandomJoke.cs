﻿using Tack.Handlers;
using Tack.Interfaces;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.BaseSet;
internal class RandomJoke : IChatCommand
{
    public Command Info() => new(
        name: "randomjoke",
        description: "Get a random joke LuL ",
        aliases: new string[] { "4head", "rj" }
        );

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        var rj = await ExternalAPIHandler.GetRandomJoke();

        if (rj is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, there was an error retrieving a random joke :(");
            return;
        }
        MessageHandler.SendMessage(channel, $"@{user}, [{rj.category}] {rj.joke.Replace('\n', ' ')} {new string[] { "LuL", "4Head", "xd", string.Empty }.Choice()}");
    }
}
