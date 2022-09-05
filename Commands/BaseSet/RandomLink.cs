﻿using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.BaseSet;
internal class RandomLink : Command
{
    public override CommandInfo Info { get; } = new(
        name: "randomlink",
        description: "Get a random link posted in Twitch chat",
        aliases: new string[] { "rl", "link" },
        userCooldown: 15,
        channelCooldown: 3
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.Username;
        string channel = ctx.IrcMessage.Channel;

        (string Username, string Channel, string Link, DateTime TimePosted) randomlink;
        using (var db = new DbQueries())
        {
            var query = await db
                .Execute(
                "SELECT * FROM collected_links " +
                "OFFSET floor(random() * (" +
                    "SELECT COUNT(*) FROM collected_links)" +
                ") LIMIT 1;");

            if (!query.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, I could not fetch a random link PoroSad");
                return;
            }

            var row = query.Results.First();
            randomlink = ((string)row[0], (string)row[1], (string)row[2], (DateTime)row[3]);
        }

        MessageHandler.SendMessage(channel, $"@{randomlink.Username} " +
            $"linked: {randomlink.Link} in #{randomlink.Channel} " +
            $"({FormatTimePosted(randomlink.TimePosted)})");
    }

    private string FormatTimePosted(DateTime time)
    {
        TimeSpan span = DateTime.Now - time.ToLocalTime();

        return span.FormatTimeLeft() + " ago";
    }
}
