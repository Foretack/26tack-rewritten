using System.Text.RegularExpressions;
using Tack.Core;
using Tack.Handlers;
using Tack.Nonclass;

namespace Tack.Modules;
internal sealed class Tf2NewsPrinter : IModule
{
    public string Name => GetType().Name;
    public bool Enabled { get; private set; }

    public Tf2NewsPrinter(bool enabled)
    {
        if (enabled)
            Enable();
    }

    private int _arrowLength;
    private readonly Regex _arrow = new(@"<:arrow:[0-9]+>|:arrow:");
    private readonly string _relayChannel = AppConfig.RelayChannel;

    private async void OnDiscordMessage(object? sender, Core.OnDiscordMsgArgs e)
    {
        Models.DiscordMessage? message = e.DiscordMessage;
        if (message is not { ChannelId: > 0 }
        || message.ChannelId != 864407160422662184
        || message.Author.Username is not "TF2 Community #updates"
        || (!message.Content.Contains("Team Fortress 2 Update Released")
        && !_arrow.IsMatch(message.Content)))
        {
            return;
        }

        if (_arrowLength == 0)
            _arrowLength = _arrow.Match(message.Content).Length;
        string[] lines = message.Content.Split('\n');
        foreach (string line in lines)
        {
            if (line.Length < _arrowLength + 4)
                continue;
            if (line[..4] == "    " && _arrow.IsMatch(line[4..(_arrowLength + 4)]))
                await MessageHandler.SendMessage(_relayChannel, "➜ " + line[(_arrowLength + 4)..]);
            else if (_arrow.IsMatch(line[.._arrowLength]))
                await MessageHandler.SendMessage(_relayChannel, "● " + line[_arrowLength..]);
            else if (line.StartsWith("https://www.teamfortress.com"))
                await MessageHandler.SendMessage(_relayChannel, line);
            else
                continue;
            await Task.Delay(500);
        }
    }

    public void Enable()
    {
        MessageHandler.OnDiscordMsg += OnDiscordMessage;
        Enabled = true;
        UpdateSettings();
        Log.Debug($"{nameof(Tf2NewsPrinter)} Module enabled");
    }

    public void Disable()
    {
        MessageHandler.OnDiscordMsg -= OnDiscordMessage;
        Enabled = false;
        UpdateSettings();
        Log.Debug($"{nameof(Tf2NewsPrinter)} Module disabled");
    }

    public void UpdateSettings()
    {
        Program.Settings.EnabledModules[Name] = Enabled;
    }
}
