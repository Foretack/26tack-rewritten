using System.Text;
using SqlKata.Execution;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.BaseSet;
internal sealed class RandomLink : Command
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
                .Append("LIMIT 1");

            var query = await db.Queue(q => q
            .SelectRaw($"* FROM collected_links {queryString}")
            .GetAsync());

            var row = query.FirstOrDefault();
            if (row is null)
            {
                MessageHandler.SendMessage(channel, $"@{user}, I could not fetch a random link PoroSad");
                return;
            }

            randomlink = ((string)row.username, (string)row.channel, (string)row.link_text, (DateTime)row.time_posted);
        }

        MessageHandler.SendMessage(channel, $"@{randomlink.Username} " +
            $"linked: {randomlink.Link} in #{randomlink.Channel} " +
            $"({FormatTimePosted(randomlink.TimePosted)})");
    }

    private string FormatTimePosted(DateTime time) => Time.SinceString(time) + " ago";
}
