using System.Text;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.BaseSet;
internal class RandomLink : Command
{
    public override CommandInfo Info { get; } = new(
        name: "randomlink",
        description: "Get a random link posted in Twitch chat. Additional options: `contains:string`, `channel:string`, `user:string`",
        aliases: new string[] { "rl", "link" },
        userCooldown: 10,
        channelCooldown: 3
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.Username;
        string channel = ctx.IrcMessage.Channel;

        string? contains = Options.ParseString("contains", ctx.IrcMessage.Message);
        string? targetUser = Options.ParseString("user", ctx.IrcMessage.Message)?.ToLower();
        string? targetChannel = Options.ParseString("channel", ctx.IrcMessage.Message)?.ToLower();

        (string Username, string Channel, string Link, DateTime TimePosted) randomlink;
        using (var db = new DbQueries())
        {
            var queryString = new StringBuilder();
            queryString.Append("SELECT * FROM collected_links ");

            var options = new[]
            {
                contains is null ? null : $"link_text LIKE '%{contains}%'",
                targetUser is null ? null : $"username LIKE '%{targetUser}%'",
                targetChannel is null ? null : $"channel LIKE '%{targetChannel}%'"
            };
            var queryConditions = new StringBuilder();
            var selectedOptions = options.Where(x => x is not null).ToArray();
            if (selectedOptions.Length > 0)
            {
                _ = queryConditions
                    .Append("WHERE ")
                    .Append(
                    string.Join(" AND ", selectedOptions));
            }

            _ = queryString.Append(queryConditions)
                .Append(" OFFSET floor")
                .Append('(')
                .Append("random()")
                .Append('*')
                .Append($"(SELECT COUNT(*) FROM collected_links {queryConditions})")
                .Append(')')
                .Append("LIMIT 1;");

            var query = await db.Execute(queryString.ToString());

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
