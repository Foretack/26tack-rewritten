using System.Text.RegularExpressions;
using Tack.Core;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Modules;
internal sealed class MentionsRelay : ChatModule
{
    public MentionsRelay(bool enabled)
    {
        if (!enabled) Disable();
        OnEnabled = x => Log.Information($"{x.Name} enabled");
        OnDisabled = x => Log.Warning($"{x.Name} has been disabled!");
    }

    private static readonly Regex _mention = new(AppConfigLoader.Config.MentionsRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    protected override async ValueTask OnMessage(TwitchMessage ircMessage)
    {
        if (!Permission.IsBlacklisted(ircMessage.Username) && _mention.IsMatch(ircMessage.Message))
        {
            string msg = $"`[{DateTime.Now:F}] #{ircMessage.Channel} {ircMessage.Username}:` {ircMessage.Message}";
            await DiscordChat.SendMessage(AppConfigLoader.Config.Mentions, msg);
        }
    }
}
