using Tack.Core;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Modules;
internal sealed class BaroChecker : IModule
{
    public string Name => GetType().Name;
    public bool Enabled { get; private set; } = true;

    public BaroChecker(bool enabled)
    {
        if (!enabled)
            Enabled = false;
        Time.DoEvery(TimeSpan.FromMinutes(15), async () => await Check()); // Always running. returns if disabled
    }

    private bool _active = false;
    private bool _scheduled = false;

    private async ValueTask Check()
    {
        if (!Enabled)
            return;
        if (_scheduled)
            return;

        VoidTrader? baro = (await ExternalApiHandler.WarframeStatusApi<VoidTrader>("voidTrader")).Value;
        if (baro is null)
            return;

        _active = baro.Active;
        if (!_active && !baro.Active)
        {
            if (Time.HasPassed(baro.Activation))
                return;
            Time.Schedule(async () =>
            {
                await ArrivedEv(baro);
                _scheduled = false;
            }, Time.Until(baro.Activation));
            _scheduled = true;
            Log.Debug("Scheduled baro arrival: {time}", Time.UntilString(baro.Activation));
        }

        if (_active && baro.Active)
        {
            if (Time.HasPassed(baro.Expiry))
                return;
            Time.Schedule(async () =>
            {
                await DepartedEv();
                _scheduled = false;
            }, Time.Until(baro.Expiry));
            _scheduled = true;
            Log.Debug("Scheduled baro departure: {time}", Time.UntilString(baro.Expiry));
        }
    }

    private async Task ArrivedEv(VoidTrader baro)
    {
        _active = true;
        if (!Enabled)
            return;
        await MessageHandler.SendMessage("pajlada", $"Void trader Baro Ki’Teer has arrived at {baro.Location}! 💠");
        await MessageHandler.SendMessage(AppConfigLoader.Config.RelayChannel, $"Void trader Baro Ki’Teer has arrived at {baro.Location}! 💠");
    }

    private async Task DepartedEv()
    {
        _active = false;
        if (!Enabled)
            return;
        await MessageHandler.SendMessage("pajlada", "Void trader Baro Ki’Teer has departed! 💠");
        await MessageHandler.SendMessage(AppConfigLoader.Config.RelayChannel, "Void trader Baro Ki’Teer has departed! 💠");
    }

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

    public void UpdateSettings()
    {
        Program.Settings.EnabledModules[Name] = Enabled;
    }
}
