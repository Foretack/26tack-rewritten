using System.Text.RegularExpressions;
using SqlKata.Execution;
using Tack.Core;
using Tack.Database;
using Tack.Handlers;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Modules;
internal sealed class LinkCollection : ChatModule
{
    public LinkCollection()
    {
        AnonymousChat.OnMessage += OnMessage;

        OnEnabled = _ => AnonymousChat.OnMessage += OnMessage;
        OnDisabled = _ => AnonymousChat.OnMessage -= OnMessage;
        Time.DoEvery(10, async () => await Commit());
    }

    // no point in setting a timeout; timeouts throw exceptions, and exceptions can't be caught in async void
    private static readonly Regex _regex = new(@"https?:[\\/][\\/](www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\=]*)", RegexOptions.Compiled);
    private static readonly List<(string Username, string Channel, string Link)>[] _commitLists = new[]
    {
        new List<(string, string, string)>(),
        new List<(string, string, string)>()
    };
    private bool _toggle = false;

    private void OnMessage(object? sender, OnMessageArgs e)
    {
        var ircMessage = e.ChatMessage;
        if (ircMessage.Message.Length < 10) return;
        if (ircMessage.Username == Config.Auth.Username) return;
        if (ircMessage.Username.Contains("bot") || ircMessage.Username == "streamelements") return;
        if (ChannelHandler.FetchedChannels.Any(x => !x.Logged && x.Username == ircMessage.Channel)) return;

        string? link = ircMessage.Message.Split(' ').FirstOrDefault(x => _regex.IsMatch(x));
        if (link is null) return;
        if (link.Length < 10) return;
        if (link.Length > 400) return;

        _commitLists[_toggle ? 0 : 1].Add((ircMessage.Username, ircMessage.Channel, link));
    }

    private async Task Commit()
    {
        _toggle = !_toggle;
        using (DbQueries db = new DbQueries())
        {
            var list = _commitLists[_toggle ? 1 : 0];
            if (!list.Any() || list.Count == 0) return;
            _ = await db["collected_links"].InsertAsync(list.Select(x =>
            {
                return new
                {
                    username = x.Username,
                    channel = x.Channel,
                    link_text = x.Link
                };
            }));
            list.Clear();
        }
    }
}
