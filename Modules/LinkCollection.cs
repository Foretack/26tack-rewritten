using System.Text.RegularExpressions;
using SqlKata.Execution;
using Tack.Core;
using Tack.Database;
using Tack.Handlers;
using Tack.Nonclass;
using TwitchLib.Client.Events;

namespace Tack.Modules;
internal class LinkCollection : ChatModule
{
    public LinkCollection()
    {
        AnonymousClient.Client.OnMessageReceived += OnMessage;

        OnEnabled = _ => AnonymousClient.Client.OnMessageReceived += OnMessage;
        OnDisabled = _ => AnonymousClient.Client.OnMessageReceived -= OnMessage;
    }

    private static readonly Regex _regex = new(@"https?:[\\/][\\/](www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\=]*)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

    private async void OnMessage(object? sender, OnMessageReceivedArgs e)
    {
        var ircMessage = e.ChatMessage;
        if (ircMessage.Message.Length < 10) return;
        if (ircMessage.Username == Config.Auth.Username) return;
        if (ircMessage.Username.Contains("bot") || ircMessage.Username == "streamelements") return;
        if (ChannelHandler.FetchedChannels.Any(x => !x.Logged && x.Name == ircMessage.Channel)) return;

        string? link = ircMessage.Message.Split(' ').FirstOrDefault(x => _regex.IsMatch(x));
        if (link is null) return;
        if (link.Length < 10) return;
        if (link.Length > 400) return;

        using (DbQueries db = new DbQueries())
        {
            _ = await db.QueryFactory.Query("collected_links").InsertAsync(new
            {
                username = ircMessage.Username,
                channel = ircMessage.Channel,
                link_text = link
            });
        }
    }
}
