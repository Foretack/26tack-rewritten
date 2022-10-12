﻿using Tack.Handlers;
using Tack.Json;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Modules;
internal sealed class BaroChecker : IModule
{
    public string Name => this.GetType().Name;
    public bool Enabled { get; private set; } = true;

    public BaroChecker()
    {
        Time.DoEvery(TimeSpan.FromMinutes(15), async () => await Check()); // Always running; returns if disabled
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
                ArrivedEv(ref baro);
                _scheduled = false;
            }, baro.Activation);
            _scheduled = true;
            Log.Debug($"Scheduled baro arrival: {Time.UntilString(baro.Activation)}");
        }

        if (_active && baro.Active)
        {
            if (Time.HasPassed(baro.Expiry)) return;
            Time.Schedule(() =>
            {
                DepartedEv(ref baro);
                _scheduled = false;
            }, baro.Expiry);
            _scheduled = true;
            Log.Debug($"Scheduled baro departure: {Time.UntilString(baro.Expiry)}");
        }
    }

    private void ArrivedEv(ref VoidTrader baro)
    {
        _active = true;
        if (!Enabled) return;
        MessageHandler.SendMessage("pajlada", $"Void trader Baro Ki’Teer has arrived at {baro.Location}! 💠");
        MessageHandler.SendMessage(Config.RelayChannel, $"Void trader Baro Ki’Teer has arrived at {baro.Location}! 💠");
    }

    private void DepartedEv(ref VoidTrader baro)
    {
        _active = false;
        if (!Enabled) return;
        MessageHandler.SendMessage("pajlada", "Void trader Baro Ki’Teer has departed! 💠");
        MessageHandler.SendMessage(Config.RelayChannel, "Void trader Baro Ki’Teer has departed! 💠");
    }

    public void Enable()
    {
        Log.Debug($"Enabled {Name}");
        Enabled = true;
    }

    public void Disable()
    {
        Log.Debug($"Disabled {Name}");
        Enabled = false;
    }
}
