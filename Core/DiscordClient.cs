using System.Text.Json;
using Tack.Database;
using Tack.Models;

namespace Tack.Core;
internal static class DiscordClient
{
    private static readonly DiscordChat _discordChat = new();
    public static void Initialize()
    {
        Redis.Subscribe("discord:messages").OnMessage(x =>
        {
            if (!x.Message.HasValue) return;
            var message = JsonSerializer.Deserialize<DiscordMessage>(x.Message!);
            if (message is null) return;
            _discordChat.Raise(message);
        });
    }
}

internal sealed class DiscordChat
{
    public delegate void OnDiscordMsgHandler(object? sender, OnDiscordMsgArgs args);
    public static event EventHandler<OnDiscordMsgArgs> OnMessage;
    public void Raise(DiscordMessage message)
    {
        RaiseEvent(new OnDiscordMsgArgs(message));
    }
    private void RaiseEvent(OnDiscordMsgArgs args)
    {
        EventHandler<OnDiscordMsgArgs> handler = OnMessage;
        if (handler is not null) handler(this, args);
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
