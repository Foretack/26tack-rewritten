using System.Text.Json;
using AsyncAwaitBestPractices;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;

namespace Tack.Core;
internal static class AnonymousClient
{
    public static string ShardStatus { get; private set; }

    private static readonly AnonymousChat _anonChat = new();
    private static readonly ShardUpdates _shardUpdates = new();
    public static void Initialize()
    {
        Redis.Subscribe("twitch:messages").OnMessage(x =>
        {
            if (!x.Message.HasValue) return;
            TwitchMessage? message = JsonSerializer.Deserialize<TwitchMessage>(x.Message!);
            if (message is null) return;
            _anonChat.Raise(message);
        });
        Redis.Subscribe("shard:updates").OnMessage(x =>
        {
            if (!x.Message.HasValue) return;
            _shardUpdates.Raise(x.Message!);
        });
        Redis.Subscribe("shard:manage").OnMessage(x =>
        {
            if (x.Message.ToString().Contains("active,")) ShardStatus = x.Message.ToString() ?? "unknown";
        });
        ShardUpdates.OnShardUpdate += (s, e) => MessageHandler.SendMessage(Config.RelayChannel, e.UpdateMessage);
    }
}

internal sealed class AnonymousChat
{
    public static WeakEventManager<OnMessageArgs> TwitchMessageManager { get; } = new();
    public delegate void OnMessageHandler(object? sender, OnMessageArgs args);

    public void Raise(TwitchMessage message)
    {
        TwitchMessageManager.RaiseEvent(this, new OnMessageArgs(message), nameof(MessageHandler.OnTwitchMsg));
    }
}

public sealed class OnMessageArgs : EventArgs
{
    public TwitchMessage ChatMessage { get; private set; }

    public OnMessageArgs(TwitchMessage twitchMessage)
    {
        ChatMessage = twitchMessage;
    }
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

    public OnShardUpdateArgs(string updateMessage)
    {
        UpdateMessage = updateMessage;
    }
}
