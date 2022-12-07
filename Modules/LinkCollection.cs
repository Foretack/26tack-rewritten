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

    private static readonly Regex _regex = new(@"https?:[\\/][\\/](www\.|[-a-zA-Z0-9]+\.)?[-a-zA-Z0-9@:%._\+~#=]{3,}(\.[a-zA-Z]{2,})+(/([-a-zA-Z0-9@:%._\+~#=/]+)?)?\b", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private static readonly List<(string Username, string Channel, string Link)>[] _commitLists = new[]
    {
        new List<(string, string, string)>(),
        new List<(string, string, string)>()
    };
    private static readonly IEnumerable<string> _columns = new[] { "username", "channel", "link_text" };
    private bool _toggle = false;

    protected override ValueTask OnMessage(TwitchMessage ircMessage)
    {
        if (ircMessage.Message.Length < 10
        || ircMessage.Username == AppConfigLoader.Config.BotUsername
        || ircMessage.Username.Contains("bot")
        || ircMessage.Username == "streamelements"
        || ChannelHandler.FetchedChannels.Any(x => !x.Logged && x.Username == ircMessage.Channel))
            return default;

        string? link = _regex.Match(ircMessage.Message).Value;
        if (link is null
        || link.Length < 10
        || link.Length > 400
        || !link.StartsWith('h'))
            return default;

        _commitLists[_toggle ? 0 : 1].Add((ircMessage.Username, ircMessage.Channel, link));

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
