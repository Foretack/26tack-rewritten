using Tack.Core;
using Tack.Handlers;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Modules;
internal sealed class Fish : IModule
{

    public string Name => GetType().Name;
    public bool Enabled { get; private set; }

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

    public Fish(bool enabled)
    {
        if (enabled)
            Enable();
        Time.DoEvery(TimeSpan.FromHours(1), TryFish);
    }

    private async Task TryFish()
    {
        if (!Enabled)
            return;
        if (Rng(9) != 0)
            return;

        bool includeEmotes = Rng();
        await Task.Delay(TimeSpan.FromSeconds(Rng(10, 1800)));
        MessageHandler.SendMessage("pajlada",
            $"$$fish {(includeEmotes ? _emotes.Choice() : null)}");
    }

    private bool Rng() => Rng(2) == 1;
    private int Rng(int end) => Rng(0, end);
    private int Rng(int start, int end) => Random.Shared.Next(start, end);

    public void Enable()
    {
        Enabled = true;
        UpdateSettings();
        Log.Debug("Enabled {name}", Name);
    }

    public void Disable()
    {
        Enabled = false;
        UpdateSettings();
        Log.Debug("Disabled {name}", Name);
    }

    private void UpdateSettings() => Program.Settings.EnabledModules[Name] = Enabled;
}