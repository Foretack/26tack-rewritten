using AsyncAwaitBestPractices;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;

namespace Tack.Core;
internal static class AnonymousClient
{
    public static string? ShardStatus { get; private set; }

    private static readonly AnonymousChat _anonChat = new();
    private static readonly ShardUpdates _shardUpdates = new();
    public static async Task Initialize()
    {
        await Redis.PubSub.SubscribeAsync<TwitchMessage>("twitch:messages", x =>
        {
            if (x is null)
                return;
            _anonChat.Raise(x);
        }).ConfigureAwait(false);

        await Redis.PubSub.SubscribeAsync<string>("shard:updates", x =>
        {
            if (string.IsNullOrEmpty(x))
                return;
            _shardUpdates.Raise(x);
        }).ConfigureAwait(false);

        ShardUpdates.OnShardUpdate += (s, e) => MessageHandler.SendMessage(AppConfigLoader.Config.RelayChannel, e.UpdateMessage);
    }
}

internal sealed class AnonymousChat
{
    public static WeakEventManager<OnMessageArgs> TwitchMessageManager { get; } = new();
    public delegate void OnMessageHandler(object? sender, OnMessageArgs args);

    public void Raise(TwitchMessage message) => TwitchMessageManager.RaiseEvent(this, new OnMessageArgs(message), nameof(MessageHandler.OnTwitchMsg));
}

public sealed class OnMessageArgs : EventArgs
{
    public TwitchMessage ChatMessage { get; private set; }

    public OnMessageArgs(TwitchMessage twitchMessage) => ChatMessage = twitchMessage;
}

internal sealed class ShardUpdates
{
    public delegate void OnShardUpdateHandler(object? sender, OnShardUpdateArgs args);
    public static event EventHandler<OnShardUpdateArgs> OnShardUpdate;
    public void Raise(string Updatemessage)
    {
        EventHandler<OnShardUpdateArgs> raiseEvent = OnShardUpdate;
        if (raiseEvent is not null)
        {
            raiseEvent(this, new OnShardUpdateArgs(Updatemessage));
        }
    }
}

public sealed class OnShardUpdateArgs : EventArgs
{
    public string UpdateMessage { get; private set; }

    public OnShardUpdateArgs(string updateMessage) => UpdateMessage = updateMessage;
}