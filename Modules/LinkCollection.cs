using System.Text.RegularExpressions;
using SqlKata.Execution;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Modules;
internal sealed class LinkCollection : ChatModule
{
    public LinkCollection()
    {
        Time.DoEvery(10, async () => await Commit());
    }

    private static readonly Regex _regex = new(@"https?:[\\/][\\/](www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\=]*)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private static readonly List<(string Username, string Channel, string Link)>[] _commitLists = new[]
    {
        new List<(string, string, string)>(),
        new List<(string, string, string)>()
    };
    private static readonly IEnumerable<string> _columns = new[] { "username", "channel", "link_text" };
    private bool _toggle = false;

    protected override ValueTask OnMessage(TwitchMessage ircMessage)
    {
        var start = DateTime.Now;
        if (ircMessage.Message.Length < 10
        || ircMessage.Username == AppConfigLoader.Config.BotUsername
        || ircMessage.Username.Contains("bot")
        || ircMessage.Username == "streamelements"
        || ChannelHandler.FetchedChannels.Any(x => !x.Logged && x.Username == ircMessage.Channel))
            return default;

        string? link = ircMessage.Message.Split(' ').FirstOrDefault(x => _regex.IsMatch(x));
        if (link is null
        || link.Length < 10
        || link.Length > 400
        || !link.StartsWith('h'))
            return default;

        _commitLists[_toggle ? 0 : 1].Add((ircMessage.Username, ircMessage.Channel, link));

        if (Time.Since(start).TotalMilliseconds >= 25) Log.Warning($"{nameof(LinkCollection)} module took too long to process a message (>=25ms)");
        return default;
    }

    private async Task Commit()
    {
        _toggle = !_toggle;
        using (DbQueries db = new DbQueries())
        {
            var list = _commitLists[_toggle ? 1 : 0];
            if (!list.Any() || list.Count == 0) return;
            var data = list.Select(x => new object[] { x.Username, x.Channel, x.Link });
            _ = await db["collected_links"].InsertAsync(_columns, data);
            list.Clear();
        }
    }
}
