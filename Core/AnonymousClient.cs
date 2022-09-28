﻿using System.Text.Json;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;

namespace Tack.Core;
internal static class AnonymousClient
{
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
        ShardUpdates.OnShardUpdate += (s, e) => MessageHandler.SendMessage(Config.RelayChannel, e.UpdateMessage);
    }
}

public sealed class AnonymousChat
{
    public delegate void OnMessageHandler(object? sender, OnMessageArgs args);
    public static event EventHandler<OnMessageArgs> OnMessage;
    public void Raise(TwitchMessage message)
    {
        RaiseEvent(new OnMessageArgs(message));
    }
    internal void RaiseEvent(OnMessageArgs args)
    {
        EventHandler<OnMessageArgs> raiseEvent = OnMessage;
        if (raiseEvent is not null)
        {
            raiseEvent(this, args);
        }
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

public sealed class ShardUpdates
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
