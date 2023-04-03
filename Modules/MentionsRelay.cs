using System.Text.RegularExpressions;
using MiniTwitch.Irc.Models;
using Tack.Core;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Modules;
internal sealed class MentionsRelay : ChatModule
{
    public MentionsRelay(bool enabled)
    {
        if (!enabled)
            Disable();
        OnEnabled = x => Log.Information($"{x.Name} enabled");
        OnDisabled = x => Log.Warning($"{x.Name} has been disabled!");
    }

    private static readonly Regex _mention = new(AppConfigLoader.Config.MentionsRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    protected override async ValueTask OnMessage(Privmsg message)
    {
        if (!Permission.IsBlacklisted(message.Author.Name) && _mention.IsMatch(message.Content))
        {
            string msg = $"`[{DateTime.Now:F}] #{message.Channel} {message.Author.Name}:` {message.Content}";
            await DiscordChat.SendMessage(AppConfigLoader.Config.Mentions, msg);
        }
    }
}
