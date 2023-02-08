using AsyncAwaitBestPractices;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;

namespace Tack.Core;
internal static class DiscordClient
{
    private static readonly DiscordChat _discordChat = new();
    private static readonly DiscordPresences _discordPresences = new();
    public static async Task Initialize()
    {
        await Redis.PubSub.SubscribeAsync<DiscordMessage>("discord:messages", x =>
        {
            if (x is null)
                return;
            _discordChat.Raise(x);
        }).ConfigureAwait(false);

        await Redis.PubSub.SubscribeAsync<DiscordPresence>("discord:presences", x =>
        {
            if (x is null)
                return;
            if (!x.Activities.Any(x => x is not null))
                return;
            _discordPresences.Raise(x);
        }).ConfigureAwait(false);
    }
}

internal sealed class DiscordChat
{
    public static WeakEventManager<OnDiscordMsgArgs> DiscordMessageManager { get; } = new();
    public delegate void OnDiscordMsgHandler(object? sender, OnDiscordMsgArgs args);

    public void Raise(DiscordMessage message) => DiscordMessageManager.RaiseEvent(this, new OnDiscordMsgArgs(message), nameof(MessageHandler.OnDiscordMsg));

    public static async Task SendMessage(ulong channelId, string content) => await Redis.PubSub.PublishAsync("discord:messages:send", new { ToChannelId = channelId, Content = content });
}

public sealed class OnDiscordMsgArgs : EventArgs
{
    public DiscordMessage DiscordMessage { get; private set; }

    public OnDiscordMsgArgs(DiscordMessage discordMessage) => DiscordMessage = discordMessage;
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
    public void Raise(DiscordPresence presence) => _presenceEventManager.RaiseEvent(this, new OnDiscordPresenceArgs(presence), nameof(OnUpdate));
}

public sealed class OnDiscordPresenceArgs : EventArgs
{
    public DiscordPresence DiscordPresence { get; private set; }

    public OnDiscordPresenceArgs(DiscordPresence discordPresence) => DiscordPresence = discordPresence;
}