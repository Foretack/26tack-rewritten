using System.Text.RegularExpressions;
using Serilog;
using Tack.Core;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using TwitchLib.Client.Events;

namespace Tack.Modules;
internal class MentionsRelay : ChatModule
{
    public MentionsRelay()
    {
        AnonymousClient.Client.OnMessageReceived += OnMessage;

        OnEnabled = x =>
        {
            Log.Information($"{x.Name} enabled");
            AnonymousClient.Client.OnMessageReceived += OnMessage;
        };
        OnDisabled = x =>
        {
            Log.Warning($"{x.Name} has been disabled!");
            AnonymousClient.Client.OnMessageReceived -= OnMessage;
        };
    }

    private static readonly Regex Mention = new(@"4s?tac?k|fo(re?[esk]|ur|r)tr?ac?k|129708505|\btest(ing)? ?(guy|individual)|login_unavailable|783267696|occluder", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private async void OnMessage(object? sender, OnMessageReceivedArgs e)
    {
        var ircMessage = e.ChatMessage;
        if (!Permission.IsBlacklisted(ircMessage.Username) && Mention.IsMatch(ircMessage.Message))
        {
            string msg = $"`[{DateTime.Now:F}] #{ircMessage.Channel} {ircMessage.Username}:` {ircMessage.Message}";
            await MessageHandler.SendDiscordMessage(Config.Discord.GuildID, Config.Discord.PingsChannelID, msg);
        }
    }
}
