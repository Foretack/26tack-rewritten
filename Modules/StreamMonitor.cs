using Tack.Core;
using Tack.Database;
using Tack.Handlers;
using Tack.Misc;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;
using TwitchLib.Api.Helix.Models.Chat;
using TwitchLib.Api.Services;
using Streams = System.Collections.Generic.Dictionary<string, Tack.Models.TwitchStream>;
using TwitchLibStream = TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream;

namespace Tack.Modules;

internal sealed class StreamMonitor : IModule
{
    public string Name => GetType().Name;
    public bool Enabled { get; private set; }
    public Streams TwitchStreams { get; private set; }

    private readonly LiveStreamMonitorService _ms = new(TwitchApiHandler.Instance.Api, 60);

    public StreamMonitor(bool enabled)
    {
        if (enabled)
            Enable();

        TwitchStreams = ChannelHandler.FetchedChannels.Where(x => x.Priority >= 0).ToDictionary(
            x => x.Username,
            y => new TwitchStream(y.Username, false, string.Empty, string.Empty, DateTime.Now));
        _ms.SetChannelsByName(ChannelHandler.FetchedChannels.Where(x => x.Priority >= 0).Select(x => x.Username).ToList());

        _ms.OnServiceStarted += (_, _) => Log.Information("[{h}] Initialized", Name);
        _ms.OnStreamOnline += StreamOnline;
        _ms.OnStreamUpdate += StreamUpdate;
        _ms.OnStreamOffline += StreamOffline;

        Time.DoEvery(TimeSpan.FromMinutes(5), async () => await Redis.Cache.SetObjectAsync("twitch:channels:streams", TwitchStreams.Select(x => x.Value)));
    }

    private async void StreamOffline(object? sender, TwitchLib.Api.Services.Events.LiveStreamMonitor.OnStreamOfflineArgs e)
    {
        Log.Information("[{header}] {channel} has gone offline!", Name, e.Channel);
        TimeSpan uptime = Time.Since(TwitchStreams[e.Channel].Started);
        UpdateDict(e.Channel, e.Stream, nameof(StreamOffline));

        await MessageHandler.SendColoredMessage(
            AppConfig.RelayChannel,
            $"{RandomReplies.StreamOfflineEmotes.Choice()} @{e.Channel} is now offline! -- {uptime.FormatTimeLeft()}",
            UserColors.GoldenRod);
    }

    private async void StreamUpdate(object? sender, TwitchLib.Api.Services.Events.LiveStreamMonitor.OnStreamUpdateArgs e)
    {
        Log.Debug("[{header}] {channel} tick", Name, e.Channel);
        if (!TwitchStreams.ContainsKey(e.Channel))
        {
            TwitchStreams.Add(e.Channel, new(e.Stream.UserName, true, e.Stream.Title, e.Stream.GameName, e.Stream.StartedAt));
        }

        if (TwitchStreams[e.Channel].Title != e.Stream.Title
        || TwitchStreams[e.Channel].GameName != e.Stream.GameName)
        {
            TimeSpan uptime = Time.Since(TwitchStreams[e.Channel].Started);
            UpdateDict(e.Channel, e.Stream, nameof(StreamUpdate));

            await MessageHandler.SendColoredMessage(
                AppConfig.RelayChannel,
                $"{RandomReplies.StreamUpdateEmotes.Choice()} @{e.Channel} updated their stream: {e.Stream.Title} -- {e.Stream.GameName} -- {uptime.FormatTimeLeft()}",
                UserColors.DodgerBlue);
        }
    }

    private async void StreamOnline(object? sender, TwitchLib.Api.Services.Events.LiveStreamMonitor.OnStreamOnlineArgs e)
    {
        Log.Information("[{header}] {channel} has gone live!", Name, e.Channel);
        UpdateDict(e.Channel, e.Stream, nameof(StreamOnline));

        await MessageHandler.SendColoredMessage(
            AppConfig.RelayChannel,
            $"{RandomReplies.StreamOnlineEmotes.Choice()} @{e.Channel} has gone live: {e.Stream.Title} - {e.Stream.GameName}",
            UserColors.SpringGreen);
    }

    private void UpdateDict(string channel, TwitchLibStream stream, string type)
    {
        switch (type)
        {
            case nameof(StreamOnline):
                if (!TwitchStreams.ContainsKey(channel))
                {
                    TwitchStreams.Add(channel, new(stream.UserName, true, stream.Title, stream.GameName, stream.StartedAt));
                }
                else
                {
                    TwitchStreams[channel].IsOnline = true;
                    TwitchStreams[channel].Title = stream.Title;
                    TwitchStreams[channel].GameName = stream.GameName;
                    TwitchStreams[channel].Started = stream.StartedAt;
                }

                break;

            case nameof(StreamOffline):
                TwitchStreams[channel].IsOnline = false;
                break;

            case nameof(StreamUpdate):
                TwitchStreams[channel].Title = stream.Title;
                TwitchStreams[channel].GameName = stream.GameName;
                break;
        }
    }

    public void Enable()
    {
        Enabled = true;
        UpdateSettings();
        _ms.Start();
        Log.Information("[{name}] Started", Name);
    }

    public void Disable()
    {
        Enabled = false;
        UpdateSettings();
        _ms.Stop();
        Log.Debug("Disabled {name} Stopped", Name);
    }

    public void UpdateSettings()
    {
        Program.Settings.EnabledModules[Name] = Enabled;
    }
}
