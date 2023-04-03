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
        string user = ctx.Message.Author.Name;
        string channel = ctx.Message.Channel.Name;

        string? contains = Options.ParseString("contains", ctx.Message.Content);
        string? targetUser = Options.ParseString("user", ctx.Message.Content)?.ToLower();
        string? targetChannel = Options.ParseString("channel", ctx.Message.Content)?.ToLower();

        (string Username, string Channel, string Link, DateTime TimePosted) randomlink;
        using (DbQueries db = new SingleOf<DbQueries>())
        {
            var queryString = new StringBuilder();

            string?[] options = new[]
            {
                contains is null ? null : $"link_text LIKE '%{contains}%'",
                targetUser is null ? null : $"username LIKE '%{targetUser}%'",
                targetChannel is null ? null : $"channel LIKE '%{targetChannel}%'"
            };
            var queryConditions = new StringBuilder();
            string?[] selectedOptions = options.Where(x => x is not null).ToArray();
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

            IEnumerable<dynamic> query = await db.Enqueue(q => q
            .SelectRaw($"* FROM collected_links {queryString}")
            .GetAsync());

            dynamic? row = query.FirstOrDefault();
            if (row is null)
            {
                await MessageHandler.SendMessage(channel, $"@{user}, I could not fetch a random link PoroSad");
                return;
            }

            randomlink = ((string)row.username, (string)row.channel, (string)row.link_text, (DateTime)row.time_posted);
        }

        await MessageHandler.SendMessage(channel,
            $"@{randomlink.Username} "
            + $"linked: {randomlink.Link} in #{randomlink.Channel} "
            + $"({FormatTimePosted(randomlink.TimePosted)})");
    }

    private static string FormatTimePosted(DateTime time)
    {
        return Time.SinceString(time) + " ago";
    }
}
