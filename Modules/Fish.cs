using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Modules;
internal sealed class Fish : ChatModule
{
    private readonly string[] _emotes =
    {
        "ApuApustaja",
        "ApuApustaja TeaTime",
        "peepoSitFishing",
        "peepoSitFishing miniW",
        "miniDank",
        "miniDank MiniTeaTime",
        "miniS MiniTeaTime",
        "miniS",
        "paaaajaW"
    };

    protected override async ValueTask OnMessage(TwitchMessage ircMessage)
    {
        if (ircMessage.Channel != "supinic" && ircMessage.Channel != "pajlada") return;
        if (!ircMessage.Message.StartsWith("$$fish")
        && !ircMessage.Message.StartsWith("$$ fish")) return;
        if (Rng(100) != 0) return;

        bool includeEmotes = Rng();
        await Task.Delay(TimeSpan.FromSeconds(Rng(10, 600)));
        MessageHandler.SendMessage(ircMessage.Channel,
            $"$$fish {(includeEmotes ? _emotes.Choice() : null)}");
    }

    private bool Rng() => Rng(2) == 1;
    private int Rng(int end) => Rng(0, end);
    private int Rng(int start, int end) => Random.Shared.Next(start, end);
}
