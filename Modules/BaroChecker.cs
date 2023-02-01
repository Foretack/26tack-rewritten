using Tack.Core;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Modules;
internal sealed class BaroChecker : IModule
{
    public string Name => this.GetType().Name;
    public bool Enabled { get; private set; } = true;

    public BaroChecker(bool enabled)
    {
        if (!enabled) Enabled = false;
        Time.DoEvery(TimeSpan.FromMinutes(15), async () => await Check()); // Always running. returns if disabled
    }

    private bool _active = false;
    private bool _scheduled = false;

    private async ValueTask Check()
    {
        if (!Enabled) return;
        if (_scheduled) return;

        VoidTrader? baro = (await ExternalAPIHandler.WarframeStatusApi<VoidTrader>("voidTrader")).Value;
        if (baro is null) return;

        _active = baro.Active;
        if (!_active && !baro.Active)
        {
            if (Time.HasPassed(baro.Activation)) return;
            Time.Schedule(() =>
            {
                ArrivedEv(baro);
                _scheduled = false;
            }, baro.Activation);
            _scheduled = true;
            Log.Debug("Scheduled baro arrival: {time}", Time.UntilString(baro.Activation));
        }

        if (_active && baro.Active)
        {
            if (Time.HasPassed(baro.Expiry)) return;
            Time.Schedule(() =>
            {
                DepartedEv(baro);
                _scheduled = false;
            }, baro.Expiry);
            _scheduled = true;
            Log.Debug("Scheduled baro departure: {time}", Time.UntilString(baro.Expiry));
        }
    }

    private void ArrivedEv(VoidTrader baro)
    {
        _active = true;
        if (!Enabled) return;
        MessageHandler.SendMessage("pajlada", $"Void trader Baro Ki’Teer has arrived at {baro.Location}! 💠");
        MessageHandler.SendMessage(AppConfigLoader.Config.RelayChannel, $"Void trader Baro Ki’Teer has arrived at {baro.Location}! 💠");
    }

    private void DepartedEv(VoidTrader baro)
    {
        _active = false;
        if (!Enabled) return;
        MessageHandler.SendMessage("pajlada", "Void trader Baro Ki’Teer has departed! 💠");
        MessageHandler.SendMessage(AppConfigLoader.Config.RelayChannel, "Void trader Baro Ki’Teer has departed! 💠");
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

    private void UpdateSettings()
    {
        Program.Settings.EnabledModules[Name] = Enabled;
    }
}
