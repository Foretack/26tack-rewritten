using System.Text.RegularExpressions;
using Serilog;
using Tack.Core;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Modules;
internal sealed class MentionsRelay : ChatModule
{
    public MentionsRelay()
    {
        AnonymousChat.OnMessage += OnMessage;

        OnEnabled = x =>
        {
            Log.Information($"{x.Name} enabled");
            AnonymousChat.OnMessage += OnMessage;
        };
        OnDisabled = x =>
        {
            Log.Warning($"{x.Name} has been disabled!");
            AnonymousChat.OnMessage -= OnMessage;
        };
    }

    private static readonly Regex Mention = new(@"4s?tac?k|fo(re?[esk]|ur|r)tr?ac?k|129708505|\btest(ing)? ?(guy|individual)|login_unavailable|783267696|occluder", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private async void OnMessage(object? sender, OnMessageArgs e)
    {
        var ircMessage = e.ChatMessage;
        if (!Permission.IsBlacklisted(ircMessage.Username) && Mention.IsMatch(ircMessage.Message))
        {
            string msg = $"`[{DateTime.Now:F}] #{ircMessage.Channel} {ircMessage.Username}:` {ircMessage.Message}";
            await MessageHandler.SendDiscordMessage(Config.Discord.GuildID, Config.Discord.PingsChannelID, msg);
        }
    }
}
