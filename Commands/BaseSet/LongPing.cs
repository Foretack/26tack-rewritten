using System.Text;
using Tack.Core;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.BaseSet;
internal class LongPing : Command
{
    public override CommandInfo Info { get; } = new(
    name: "longping",
    description: "Test if TMI is in fact eating messages",
    userCooldown: 300,
    channelCooldown: 300,
    permission: PermissionLevels.Moderator
    );

    private static bool Commencing { get; set; } = false;
    private static readonly List<string> NotifyList = new List<string>();

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        if (Commencing)
        {
            MessageHandler.SendMessage(channel, $"@{user}, A test is already commencing WTRuck You will be notified about its results");
            if (!NotifyList.Contains(channel)) NotifyList.Add(channel);
            return;
        }

        var prev = ObjectCache.Get<string>("longping_results");
        if (prev is not null)
        {
            MessageHandler.SendMessage(channel, $"[Cached] {prev}");
            return;
        }

        Commencing = true;
        string[] messages = await GenerateMessages();
        int count = messages.Length;
        AnonymousClient.Client.OnMessageReceived += Read;

        for (int i = 0; i < count; i++)
        {
            string message = messages[i];
            MessageHandler.SendMessage(Config.Auth.Username, $"test {i + 1} " + message);
            await Task.Delay(125);
        }
        await Task.Delay(2500);
        
        AnonymousClient.Client.OnMessageReceived -= Read;
        if (!NotifyList.Contains(channel)) NotifyList.Add(channel);

        string results = $"{CaughtCount} of {count} messages caught | ~{LatencySum / CaughtCount}ms";
        ObjectCache.Put("longping_results", results, 250);

        CaughtCount = 0;
        LatencySum = 0;

        foreach (string c in NotifyList)
        {
            MessageHandler.SendMessage(c, results);
            NotifyList.Remove(c);
        }
        Commencing = false;
    }

    private static readonly char[] chars = 
    { 
        '⣿', '⣷', '⡜', '⢀', '⠂', '⣶', '⣒', 
        'a', 'b', 'c', 'd', 'e', 'f', '1', '2',
        '3', '4', '5', '6', '7', '8', '9', '0' 
    };
    private async Task<string[]> GenerateMessages(int count = 50)
    {
        List<string> messages = new List<string>();
        await Task.Run(() =>
        {
            for (int i = 0; i < count; i++)
            {
                StringBuilder message = new StringBuilder();
                for (int j = 0; j < 450; j++)
                {
                    message.Append(chars.Choice());
                }
                messages.Add(message.ToString());
            }
        });
        return messages.ToArray();
    }

    private static short CaughtCount { get; set; } = 0;
    private static float LatencySum { get; set; } = 0;
    private void Read(object? sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
    {
        var ircMessage = e.ChatMessage;
        long unixTimeMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (ircMessage.Channel == Config.Auth.Username
        && ircMessage.Username == Config.Auth.Username)
        {
            float Latency = (float)(unixTimeMs - double.Parse(ircMessage.TmiSentTs));
            LatencySum += Latency;
            CaughtCount++;
        }
    }
}
