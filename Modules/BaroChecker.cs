using Serilog;
using Tack.Handlers;
using Tack.Json;
using Tack.Nonclass;
using Tack.Utils;
using TTimer = System.Timers.Timer;

namespace Tack.Modules;
internal sealed class BaroChecker : IModule
{
    public string Name => this.GetType().Name;
    public bool Enabled { get; private set; } = true;

    private readonly TTimer _checker = new();

    public BaroChecker()
    {
        _checker.Interval = TimeSpan.FromMinutes(15).TotalMilliseconds;
        _checker.Enabled = true;
        _checker.Elapsed += async (_, _) => await Check(); // Always running; returns if disabled
    }

    private bool Active { get; set; } = false;
    private bool Scheduled { get; set; } = false;

    private async ValueTask Check()
    {
        if (!Enabled) return;
        if (Scheduled) return;

        VoidTrader? baro = (await ExternalAPIHandler.WarframeStatusApi<VoidTrader>("voidTrader")).Value;
        if (baro is null) return;

        Active = baro.Active;
        if (!Active && !baro.Active)
        {
            TimeSpan arrivalTime = Time.Until(baro.Activation);
            Timer? timer = null;

            timer = new Timer(_ =>
            {
                ArrivedEv(ref baro);
                Scheduled = false;
                timer?.Dispose();
            },
            null, arrivalTime, Timeout.InfiniteTimeSpan);
            Scheduled = true;
            Log.Debug($"Scheduled baro arrival: {Time.UntilString(baro.Activation)}");
        }

        if (Active && baro.Active)
        {
            TimeSpan departureTime = Time.Until(baro.Expiry);
            Timer? timer = null;

            timer = new Timer(_ =>
            {
                DepartedEv(ref baro);
                Scheduled = false;
                timer?.Dispose();
            },
            null, departureTime, Timeout.InfiniteTimeSpan);
            Scheduled = true;
            Log.Debug($"Scheduled baro departure: {Time.UntilString(baro.Expiry)}");
        }
    }

    private void ArrivedEv(ref VoidTrader baro)
    {
        Active = true;
        if (!Enabled) return;
        MessageHandler.SendMessage("pajlada", $"Void trader Baro Ki’Teer has arrived at {baro.Location}! 💠");
        MessageHandler.SendMessage(Config.RelayChannel, $"Void trader Baro Ki’Teer has arrived at {baro.Location}! 💠");
    }

    private void DepartedEv(ref VoidTrader baro)
    {
        Active = false;
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
