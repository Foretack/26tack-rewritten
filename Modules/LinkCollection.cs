using System.Text.RegularExpressions;
using Tack.Core;
using Tack.Database;
using Tack.Nonclass;

namespace Tack.Modules;
internal class LinkCollection : ChatModule
{
    public LinkCollection()
    {
        AnonymousClient.Client.OnMessageReceived += OnMessage;

        OnEnabled = _ => AnonymousClient.Client.OnMessageReceived += OnMessage;
        OnDisabled = _ => AnonymousClient.Client.OnMessageReceived -= OnMessage;
    }

    private static readonly Regex _regex = new(@"(https:[\\/][\\/])?(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\=]*)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

    private async void OnMessage(object? sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
    {
        var ircMessage = e.ChatMessage;
        if (ircMessage.Username.Contains("bot")) return;

        var match = _regex.Match(ircMessage.Message);
        if (!match.Success) return;

        using (DbQueries db = new DbQueries())
        {
            _ = await db
                .Table("collected_links")
                .Insert()
                .Schema("username", "channel", "link")
                .Values(
                    $"'{ircMessage.Username}'",
                    $"'{ircMessage.Channel}'",
                    $"'{match.Value}'"
                        )
                .TryExecute();
        }
    }
}
