using Tack.Handlers;
using Tack.Nonclass;

namespace Tack.Modules;
internal sealed class Tf2NewsPrinter : IModule
{
    public string Name => this.GetType().Name;
    public bool Enabled { get; private set; }

    public Tf2NewsPrinter() => Enable();

    private const string TfArrow = "<:arrow:1032084130399264858>";

    private async void OnDiscordMessage(object? sender, Core.OnDiscordMsgArgs e)
    {
        var message = e.DiscordMessage;
        if (message is null
        || message.ChannelId == 0
        || message.Author.Name is null
        || message.ChannelId != 864407160422662184
        || message.Author.Username != "TF2 Community #updates"
        || !message.Content.StartsWith("**Team Fortress 2 Update Released**")
        || !message.Content.StartsWith(TfArrow))
            return;

        var lines = message.Content.Split('\n');
        foreach (var line in lines)
        {
            if (line[..4] == "    " && line[4..TfArrow.Length] == TfArrow)
                MessageHandler.SendMessage(Config.RelayChannel, "|-> " + line[(TfArrow.Length + 4)..]);
            else if (line.StartsWith(TfArrow))
                MessageHandler.SendMessage(Config.RelayChannel, "● " + line[TfArrow.Length..]);
            else continue;
            await Task.Delay(500);
        }
    }

    public void Enable()
    {
        MessageHandler.OnDiscordMsg += OnDiscordMessage;
        Enabled = true;
        Log.Information($"{nameof(Tf2NewsPrinter)} Module enabled");
    }

    public void Disable()
    {
        MessageHandler.OnDiscordMsg -= OnDiscordMessage;
        Enabled = false;
        Log.Information($"{nameof(Tf2NewsPrinter)} Module disabled");
    }
}
