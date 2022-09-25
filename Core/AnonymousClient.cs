using System.Text.Json;
using Tack.Database;
using Tack.Models;

namespace Tack.Core;
internal static class AnonymousClient
{
    private static readonly AnonymousChat _anonChat = new();
    public static void Initialize()
    {
        Redis.Subscribe("twitch:messages").OnMessage(x =>
        {
            if (!x.Message.HasValue) return;
            TwitchMessage? message = JsonSerializer.Deserialize<TwitchMessage>(x.Message!);
            if (message is null) return;
            _anonChat.Raise(message);
        });
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
    public TwitchMessage ChatMessage { get; set; }

    public OnMessageArgs(TwitchMessage twitchMessage)
    {
        ChatMessage = twitchMessage;
    }
}
