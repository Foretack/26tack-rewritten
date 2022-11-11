using Tack.Core;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Modules;
internal class DiscordActivityNotifier : IModule
{
    public static Activity CurrentSong { get; private set; } = default!;

    public string Name => this.GetType().Name;
    public bool Enabled { get; private set; } = true;

    public DiscordActivityNotifier() => Enable();

    private void OnUpdate(object? sender, OnDiscordPresenceArgs e)
    {
        var presence = e.DiscordPresence;
        if (presence.Author.IsBot || presence.Activities.Count == 0) return;
        foreach (var activity in presence.Activities)
        {
            if (activity is null) continue;
            bool spotify = activity.LargeImage.Id.StartsWith("spotify");

            if (spotify)
            {
                string lenString = activity.EndTimestamp is null ? string.Empty : $"{Time.Until((DateTime)activity.EndTimestamp):m'm 's's'}";
                if (!ActOnCooldown() && activity.Details != CurrentSong?.Details)
                {
                    MessageHandler.SendMessage(AppConfigLoader.Config.RelayChannel,
                        $"{presence.Author.Username} is listening to: \"{activity.Details}\" by {activity.State} [{lenString}] 🎶 ");
                }
                CurrentSong = activity;
                continue;
            }
        }
    }

    private bool _onCooldown;
    private bool ActOnCooldown()
    {
        if (!_onCooldown)
        {
            _onCooldown = true;
            Time.Schedule(() => _onCooldown = false, TimeSpan.FromMinutes(10));
            return false;
        }
        return _onCooldown;
    }

    public void Enable()
    {
        DiscordPresences.OnUpdate += OnUpdate;
        Log.Debug($"Enabled {Name}");
        Enabled = true;
    }

    public void Disable()
    {
        DiscordPresences.OnUpdate -= OnUpdate;
        Log.Debug($"Disabled {Name}");
        Enabled = false;
    }
}
