using System.Text.RegularExpressions;
using Tack.Handlers;
using Tack.Nonclass;

namespace Tack.Modules;
internal sealed class Tf2NewsPrinter : IModule
{
    public string Name => this.GetType().Name;
    public bool Enabled { get; private set; }

    public Tf2NewsPrinter() => Enable();

    private const int ArrowLength = 28;
    private readonly Regex _arrow = new(@"<:arrow:[0-9]+>");
    private readonly string _relayChannel = AppConfigLoader.Config.RelayChannel;

    private async void OnDiscordMessage(object? sender, Core.OnDiscordMsgArgs e)
    {
        var message = e.DiscordMessage;
        if (message is null
        || message.ChannelId == 0
        || message.Author.Username is null
        || message.ChannelId != 864407160422662184
        || message.Author.Username != "TF2 Community #updates"
        || (!message.Content.Contains("Team Fortress 2 Update Released")
        && !_arrow.IsMatch(message.Content)))
            return;

        var lines = message.Content.Split('\n');
        foreach (var line in lines)
        {
            if (line.Length < ArrowLength + 4) continue;
            if (line[..4] == "    " && _arrow.IsMatch(line[4..(ArrowLength + 4)]))
                MessageHandler.SendMessage(_relayChannel, "-> " + line[(ArrowLength + 4)..]);
            else if (_arrow.IsMatch(line[..ArrowLength]))
                MessageHandler.SendMessage(_relayChannel, "● " + line[ArrowLength..]);
            else if (line.StartsWith("https://www.teamfortress.com"))
                MessageHandler.SendMessage(_relayChannel, line);
            else continue;
            await Task.Delay(500);
        }
    }

    public void Enable()
    {
        MessageHandler.OnDiscordMsg += OnDiscordMessage;
        Enabled = true;
        Log.Debug($"{nameof(Tf2NewsPrinter)} Module enabled");
    }

    public void Disable()
    {
        MessageHandler.OnDiscordMsg -= OnDiscordMessage;
        Enabled = false;
        Log.Debug($"{nameof(Tf2NewsPrinter)} Module disabled");
    }
}
