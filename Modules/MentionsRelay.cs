using System.Text.RegularExpressions;
using Tack.Core;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Modules;
internal sealed class MentionsRelay : ChatModule
{
    public MentionsRelay()
    {

        OnEnabled = x =>
        {
            Log.Information($"{x.Name} enabled");
        };
        OnDisabled = x =>
        {
            Log.Warning($"{x.Name} has been disabled!");
        };
    }

    private static readonly Regex Mention = new(@"4s?tac?k|fo(re?[esk]|ur|r)tr?ac?k|129708505|\btest(ing)? ?(guy|individual)|login_unavailable|783267696|occluder", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    protected override async ValueTask OnMessage(TwitchMessage ircMessage)
    {
        var start = DateTime.Now;
        if (!Permission.IsBlacklisted(ircMessage.Username) && Mention.IsMatch(ircMessage.Message))
        {
            string msg = $"`[{DateTime.Now:F}] #{ircMessage.Channel} {ircMessage.Username}:` {ircMessage.Message}";
            await DiscordChat.SendMessage(Config.Discord.PingsChannelID, msg);
        }
        if (Time.Since(start).TotalMilliseconds >= 10) Log.Warning($"{nameof(MentionsRelay)} took too long to process a message (>=10ms)");
    }
}
