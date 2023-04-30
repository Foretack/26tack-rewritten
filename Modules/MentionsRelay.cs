using System.Text.RegularExpressions;
using MiniTwitch.Irc.Models;
using Tack.Core;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Modules;
internal sealed class MentionsRelay : ChatModule
{
    private const string SEPARATOR = "------------------------------------";
    private const string SEPARATOR_LONG = "-----------------------------------------------------------------------";
    private const string CRLF = "\r\n";

    public MentionsRelay(bool enabled)
    {
        if (!enabled)
            Disable();
        OnEnabled = x => Log.Information($"{x.Name} enabled");
        OnDisabled = x => Log.Warning($"{x.Name} has been disabled!");
    }

    private static readonly Regex _mention = new(AppConfig.MentionsRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    protected override async ValueTask OnMessage(Privmsg message)
    {
        if (!Permission.IsBlacklisted(message.Author.Name) && _mention.IsMatch(message.Content))
        {
            string user = message.Author.Name;
            string content = message.Content;
            string msg;
            if (message.Reply.HasContent)
                msg = $"```ansi{CRLF}\u001b[2;32m\u001b[0m\u001b[2;32m{DateTime.Now:F}\u001b[0m{CRLF}{SEPARATOR}{CRLF}\u001b[2;35m\u001b[0m\u001b[1;2m\u001b[1;35m@{user}\u001b[0m\u001b[0m in \u001b[1;2m\u001b[1;41m#{message.Channel.Name}\u001b[0m\u001b[0m\u001b[2;36m{CRLF}{SEPARATOR_LONG}{CRLF}Replying to\u001b[0m \u001b[2;35m\u001b[1;35m@{message.Reply.ParentUsername}\u001b[0m\u001b[2;35m\u001b[0m:{CRLF}> \u001b[2;33m{message.Reply.ParentMessage}\u001b[0m{CRLF}{SEPARATOR_LONG}{CRLF}{content}{CRLF}```";
            else
                msg = $"```ansi{CRLF}\u001b[2;32m\u001b[0m\u001b[2;32m{DateTime.Now:F}\u001b[0m{CRLF}{SEPARATOR}{CRLF}\u001b[2;35m\u001b[0m\u001b[1;2m\u001b[1;35m@{user}\u001b[0m\u001b[0m in \u001b[1;2m\u001b[1;41m#{message.Channel.Name}\u001b[0m\u001b[0m{CRLF}{SEPARATOR_LONG}{CRLF}{content}{CRLF}```";

            await DiscordChat.SendMessage(AppConfig.Mentions, msg);
        }
    }
}
