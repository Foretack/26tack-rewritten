using Tack.Core;
using Tack.Misc;
using Tack.Models;
using Tack.Utils;
using AsyncAwaitBestPractices;
using Discord.WebSocket;
using Serilog;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using Discord;

namespace Tack.Handlers;
internal static class MessageHandler
{
    private static ChatColor CurrentColor = ChatColor.FANCY_NOT_SET_STATE_NAME;

    internal static void Initialize()
    {
        AnonymousClient.Client.OnMessageReceived += OnMessageReceived;
        MainClient.Client.OnMessageSent += OnMessageSent;
        MainClient.Client.OnMessageThrottled += OnMessageThrottled;
    }

    public static void SendMessage(string channel, string message) { MainClient.Client.SendMessage(channel, message); }
    public static void SendColoredMessage(string channel, string message, ChatColor color)
    {
        if (CurrentColor != color)
        {
            MainClient.Client.SendMessage(Config.Auth.Username, $"/color {color}");
            CurrentColor = color;
        }
        MainClient.Client.SendMessage(channel, "/me " + message);
    }
    public static async Task SendDiscordMessage(ulong guildID, ulong channelID, string message)
    {
        SocketTextChannel channel = ObjectCache.Get<SocketTextChannel>(channelID + "_DISCORD_CHANNEL")
            ?? DiscordClient.Client.GetGuild(guildID).GetTextChannel(channelID);
        ObjectCache.Put(channelID + "_DISCORD_CHANNEL", channel, 36400);

        await channel.SendMessageAsync(message);
    }

    internal static Task OnDiscordMessageReceived(SocketMessage arg)
    {
        Log.Verbose($"Discord message received => {arg.Author.Username} {arg.Channel.Name}: {arg.Content}");
        HandleDiscordMessage(arg);
        return Task.CompletedTask;
    }

    private static void OnMessageThrottled(object? sender, OnMessageThrottledEventArgs e)
    {
        Log.Warning($"Message throttled: {e.Message} ({e.SentMessageCount} sent in {e.AllowedInPeriod})");
    }

    private static void OnMessageSent(object? sender, OnMessageSentArgs e)
    {
        Log.Debug($"Sent message: {e.SentMessage.Message}");
    }

    private static void OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        Log.Verbose($"#{e.ChatMessage.Channel} {e.ChatMessage.Username}: {e.ChatMessage.Message}");
        HandleIrcMessage(e.ChatMessage).SafeFireAndForget(onException: ex => Log.Error(ex, "Failed to handle message"));
    }

    private static async ValueTask HandleIrcMessage(ChatMessage ircMessage)
    {
        string message = ircMessage.Message;
        string channel = ircMessage.Channel;
        string[] splitMessage = message.Replace("󠀀", " ").Split(' ');
        string[] commandArgs = splitMessage.Skip(1).ToArray();

        if (CommandHandler.Prefixes.Any(x => message.StartsWith(x))
        && ChannelHandler.MainJoinedChannelNames.Contains(channel))
        {
            string commandName = splitMessage[0].Replace(CommandHandler.Prefixes.First(x => message.StartsWith(x)), string.Empty);
            Permission permission = new Permission(ircMessage);
            CommandContext ctx = new CommandContext(ircMessage, commandArgs, commandName, permission);
            await CommandHandler.HandleCommand(ctx);
        }
        if (channel == "pajlada"
        && ircMessage.Username == "pajbot"
        && ircMessage.IsMe
        && message.StartsWith("pajaS 🚨 ALERT"))
        {
            SendMessage("pajlada", "/me " + RandomReplies.PajbotReplies.Choice());
        }
        if (Regexes.Mention.IsMatch(message))
        {
            string msg = $"`[{DateTime.Now.ToLocalTime()}] #{ircMessage.Channel} {ircMessage.Username}:` {ircMessage.Message}";
            await SendDiscordMessage(Config.Discord.GuildID, Config.Discord.PingsChannelID, msg);
        }
    }

    private static void HandleDiscordMessage(SocketMessage socketMessage)
    {
        string content = socketMessage.Content;
        ulong channelID = socketMessage.Channel.Id;
        string author = socketMessage.Author.Username.StripDescriminator();
        var embeds = socketMessage.Embeds;
        
        if (content.Length < 5
        && embeds.Count > 0)
        {
            int embedCount = embeds.Count;
            Embed embed = embeds.First();
            content = $"{embed.Title} " +
                $"{(embed.Url is null ? string.Empty : $"( {embed.Url} )")} " +
                $"{(embedCount > 1 ? $"[+{embedCount - 1} embed(s)]" : string.Empty)}";
        }
        else if (content.Length >= 5
        && content.Length <= 450
        && embeds.Count > 0)
        {
            int embedCount = embeds.Count;
            content += $"[+{embedCount} embed(s)]";
        }

        content = content.Length >= 425 ? content[..425] + "..." : content; 
        if (channelID == Config.Discord.NewsChannelID
        && author.Contains("#api-announcements"))
        {
            SendColoredMessage("pajlada",
                                "imGlitch 🚨 " + content.Replace("@Twitch Announcements", string.Empty),
                                ChatColor.BlueViolet);
        }
        if (channelID == Config.Discord.NewsChannelID
        && author.Contains("7TV #news"))
        {
            SendColoredMessage("pajlada", "7tvM 📣 " + content, ChatColor.CadetBlue);
        }
        if (channelID == Config.Discord.NewsChannelID)
        {
            SendColoredMessage(Config.RelayChannel, $"New announcement from {author} B) 📢 {content}", ChatColor.Blue); 
        }
        
    }
} // class

internal enum ChatColor
{
    FANCY_NOT_SET_STATE_NAME,
    Blue, BlueViolet, CadetBlue,
    Chocolate, Coral, DodgerBlue,
    Firebrick, GoldenRod, Green,
    HotPink, OrangeRed, Red,
    SeaGreen, SpringGreen, YellowGreen
}
