using System.Text;
using Tack.Core;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.BaseSet;
internal sealed class LongPing : Command
{
    public override CommandInfo Info { get; } = new(
    name: "longping",
    description: "Test TMI",
    userCooldown: 300,
    channelCooldown: 300,
    permission: PermissionLevels.Moderator
    );

    private static bool _commencing = false;
    private static readonly List<string> _notifyList = new();

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        if (_commencing)
        {
            MessageHandler.SendMessage(channel, $"@{user}, A test is already commencing WTRuck You will be notified about its results");
            if (!_notifyList.Contains(channel)) _notifyList.Add(channel);
            return;
        }

        string? prev = await "bot:longping".Get();
        if (prev is not null)
        {
            MessageHandler.SendMessage(channel, $"[Cached] {prev}");
            return;
        }

        _commencing = true;
        string[] messages = await GenerateMessages();
        int count = messages.Length;
        AnonymousChat.OnMessage += Read;

        for (int i = 0; i < count; i++)
        {
            string message = messages[i];
            MessageHandler.SendMessage(Config.Auth.Username, $"test {i + 1} " + message);
            await Task.Delay(125);
        }
        await Task.Delay(2500);

        AnonymousChat.OnMessage -= Read;
        if (!_notifyList.Contains(channel)) _notifyList.Add(channel);

        string results = $"{_caughtCount} of {count} messages caught | ~{_latencySum / _caughtCount}ms";
        await "bot:longping".SetExpiringKey(results, TimeSpan.FromMinutes(2.5));

        _caughtCount = 0;
        _latencySum = 0;

        foreach (string c in _notifyList)
        {
            MessageHandler.SendMessage(c, results);
            _ = _notifyList.Remove(c);
        }
        _commencing = false;
    }

    private static readonly char[] _chars =
    {
        '⣿', '⣷', '⡜', '⢀', '⠂', '⣶', '⣒',
        'a', 'b', 'c', 'd', 'e', 'f', '1', '2',
        '3', '4', '5', '6', '7', '8', '9', '0'
    };
    private async Task<string[]> GenerateMessages(int count = 50)
    {
        var messages = new List<string>();
        await Task.Run(() =>
        {
            for (int i = 0; i < count; i++)
            {
                var message = new StringBuilder();
                for (int j = 0; j < 450; j++)
                {
                    _ = message.Append(_chars.Choice());
                }
                messages.Add(message.ToString());
            }
        });
        return messages.ToArray();
    }

    private static short _caughtCount = 0;
    private static float _latencySum = 0;
    private void Read(object? sender, OnMessageArgs e)
    {
        var ircMessage = e.ChatMessage;
        long unixTimeMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (ircMessage.Channel == Config.Auth.Username
        && ircMessage.Username == Config.Auth.Username)
        {
            float Latency = (float)(unixTimeMs - double.Parse(ircMessage.TmiSentTs));
            _latencySum += Latency;
            _caughtCount++;
        }
    }
}
