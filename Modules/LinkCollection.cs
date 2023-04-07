using System.Text.RegularExpressions;
using MiniTwitch.Irc.Models;
using SqlKata.Execution;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Modules;
internal sealed class LinkCollection : ChatModule
{
    public LinkCollection(bool enabled)
    {
        if (!enabled)
            Disable();
        Time.DoEvery(TimeSpan.FromMinutes(5), Commit);
    }

    private static readonly Regex _regex = new(@"https?:[\\/][\\/](www\.|[-a-zA-Z0-9]+\.)?[-a-zA-Z0-9@:%._\+~#=]{3,}(\.[a-zA-Z]{2,10})+(/([-a-zA-Z0-9@:%._\+~#=/?&]+)?)?\b", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
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

    protected override ValueTask OnMessage(Privmsg message)
    {
        if (message.Content.Length < 10
        || message.Author.Name == AppConfigLoader.Config.BotUsername
        || message.Author.Name.Contains("bot")
        || _bots.Contains(message.Author.Name)
        || ChannelHandler.FetchedChannels.Any(x => !x.Logged && x.Username == message.Channel.Name))
        {
            return default;
        }

        if (message.Author.Name.Length > 25)
        {
            Log.Warning("[{@header}] @{guy} <- This guy's name is longer than 25??", nameof(LinkCollection), message.Author.Name);
            return default;
        }

        string? link = _regex.Match(message.Content).Value;
        if (link is { Length: < 10 or > 400 } || link[0] != 'h')
            return default;

        List<LinkData> list = _commitLists[_toggle ? 0 : 1];
        list.Add((message.Author.Name, message.Channel.Name, link));
        Log.Verbose("[{@header}] Link added: {link} ({total})", Name, link, list.Count);

        return default;
    }

    private void Commit()
    {
        _toggle = !_toggle;
        List<LinkData> list = _commitLists[_toggle ? 1 : 0];
        if (list is { Count: < 10 })
        {
            Log.Debug("[{header}] Link list has less than 10 items. Skipping...", Name);
            return;
        }

        Log.Debug("[{@header}] Committing link list...", Name);
        IEnumerable<object[]> data = list.Select(x => new object[] { x.Username, x.Channel, x.Link });
        SingleOf<DbQueries>.Obj.Enqueue(async qf =>
        {
            int inserted = await qf.Query("collected_links").InsertAsync(_columns, data);
            Log.Debug("{l} links added", inserted);
            list.Clear();
        });
    }

    private record struct LinkData(string Username, string Channel, string Link)
    {
        public static implicit operator LinkData((string, string, string) tuple) =>
            new(tuple.Item1, tuple.Item2, tuple.Item3);
    };
}
