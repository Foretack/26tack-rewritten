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

    public DiscordActivityNotifier()
    {
        DiscordPresences.OnUpdate += OnUpdate;
    }

    private void OnUpdate(object? sender, OnDiscordPresenceArgs e)
    {
        var presence = e.DiscordPresence;
        if (presence.Author.IsBot || presence.Activities.Count == 0 || !presence.Activities.Any(x => x is not null)) return;
        foreach (var activity in presence.Activities)
        {
            if (activity is null) continue;
            bool spotify = activity.LargeImage.Id.StartsWith("spotify");

            if (spotify)
            {
                string endString = activity.EndTimestamp is null ? string.Empty : $"{Time.Until((DateTime)activity.EndTimestamp):m'm 's's'}";
                MessageHandler.SendMessage(Config.RelayChannel, $"{presence.Author.Username} is listening to: " +
                    $"\"{activity.Details}\" by {activity.State} [{((DateTime)activity.EndTimestamp!).ToLocalTime()}] 🎶 ");
                CurrentSong = activity;
                continue;
            }

        }
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
