using System.Text.Json;
using AsyncAwaitBestPractices;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;

namespace Tack.Core;
internal static class DiscordClient
{
    private static readonly DiscordChat _discordChat = new();
    private static readonly DiscordPresences _discordPresences = new();
    public static void Initialize()
    {
        Redis.Subscribe("discord:messages").OnMessage(async x =>
        {
            await Task.Run(() =>
            {
                if (!x.Message.HasValue) return;
                var message = JsonSerializer.Deserialize<DiscordMessage>(x.Message!);
                if (message is null) return;
                _discordChat.Raise(message);
            }).ConfigureAwait(false);
        });
        Redis.Subscribe("discord:presences").OnMessage(async x =>
        {
            await Task.Run(() =>
            {
                if (!x.Message.HasValue) return;
                var presence = JsonSerializer.Deserialize<DiscordPresence>(x.Message!);
                if (presence is null) return;
                if (!presence.Activities.Any(x => x is not null)) return;
                _discordPresences.Raise(presence);
            }).ConfigureAwait(false);
        });
    }
}

internal sealed class DiscordChat
{
    public static WeakEventManager<OnDiscordMsgArgs> DiscordMessageManager { get; } = new();
    public delegate void OnDiscordMsgHandler(object? sender, OnDiscordMsgArgs args);

    public void Raise(DiscordMessage message)
    {
        DiscordMessageManager.RaiseEvent(new OnDiscordMsgArgs(message), nameof(MessageHandler.OnDiscordMsg));
    }

    public static async Task SendMessage(ulong channelId, string content)
    {
        var obj = new { ToChannelId = channelId, Content = content };
        var json = JsonSerializer.Serialize(obj);
        _ = await Redis.PublishAsync("discord:messages:send", json);
    }
}

public sealed class OnDiscordMsgArgs : EventArgs
{
    public DiscordMessage DiscordMessage { get; private set; }

    public OnDiscordMsgArgs(DiscordMessage discordMessage)
    {
        DiscordMessage = discordMessage;
    }
}

internal sealed class DiscordPresences
{
    public static event EventHandler<OnDiscordPresenceArgs> OnUpdate
    {
        add => _presenceEventManager.AddEventHandler(value, nameof(OnUpdate));
        remove => _presenceEventManager.RemoveEventHandler(value, nameof(OnUpdate));
    }

    private static readonly WeakEventManager<OnDiscordPresenceArgs> _presenceEventManager = new();

    public delegate void OnDiscordPresenceHandler(object? sender, OnDiscordPresenceArgs args);
    public void Raise(DiscordPresence presence)
    {
        _presenceEventManager.RaiseEvent(new OnDiscordPresenceArgs(presence), nameof(OnUpdate));
    }
}

public sealed class OnDiscordPresenceArgs : EventArgs
{
    public DiscordPresence DiscordPresence { get; private set; }

    public OnDiscordPresenceArgs(DiscordPresence discordPresence)
    {
        DiscordPresence = discordPresence;
    }
}
