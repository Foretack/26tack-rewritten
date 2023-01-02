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
        Time.DoEvery(TimeSpan.FromMinutes(5), async () => await Commit());
    }

    private static readonly Regex _regex = new(@"https?:[\\/][\\/](www\.|[-a-zA-Z0-9]+\.)?[-a-zA-Z0-9@:%._\+~#=]{3,}(\.[a-zA-Z]{2,})+(/([-a-zA-Z0-9@:%._\+~#=/?&]+)?)?\b", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private static readonly List<LinkData>[] _commitLists = new[]
    {
        new List<LinkData>(),
        new List<LinkData>()
    };
    private static readonly string[] _bots =
    {
        "streamelements", "streamlabs", "scriptorex", "apulxd", "rewardmore", "iogging", "ttdb"
    };
    private static readonly IEnumerable<string> _columns = new[] { "username", "channel", "link_text" };
    private bool _toggle = false;

    protected override ValueTask OnMessage(TwitchMessage ircMessage)
    {
        if (ircMessage.Message.Length < 10
        || ircMessage.Username == AppConfigLoader.Config.BotUsername
        || ircMessage.Username.Contains("bot")
        || _bots.Contains(ircMessage.Username)
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
            try
            {
                _ = await db["collected_links"].InsertAsync(_columns, data);
                Log.Debug("{l} links added", list.Count);
                list.Clear();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to commit link list to DB");
                Log.Error("List size: {size}", list.Count);
            }
        }
    }

    private record struct LinkData(string Username, string Channel, string Link)
    {
        public static implicit operator LinkData((string, string, string) tuple) =>
            new LinkData(tuple.Item1, tuple.Item2, tuple.Item3);
    };
}
