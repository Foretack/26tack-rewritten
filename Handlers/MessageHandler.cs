using Discord;
using Discord.WebSocket;
using Serilog;
using Tack.Core;
using Tack.Database;
using Tack.Misc;
using Tack.Models;
using Tack.Utils;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace Tack.Handlers;
internal static class MessageHandler
{
    #region Properties
    private static ChatColor CurrentColor { get; set; } = ChatColor.FANCY_NOT_SET_STATE_NAME;
    private static DiscordEvent[] DiscordEvents { get; set; } = DbQueries.NewInstance().GetDiscordEvents();
    private static readonly Dictionary<string, string> LastSentMessage = new Dictionary<string, string>();
    #endregion

    #region Initialization
    public static void Initialize()
    {
        AnonymousClient.Client.OnMessageReceived += OnMessageReceived;
        MainClient.Client.OnMessageSent += OnMessageSent;
        MainClient.Client.OnMessageThrottled += OnMessageThrottled;
    }

    public static void ReloadDiscordTriggers() { DiscordEvents = DbQueries.NewInstance().GetDiscordEvents(); }
    #endregion

    #region Sending
    public static void SendMessage(string channel, string message) 
    {
        message = message.Length >= 500 ? message[..495] + "..." : message;
        if (!LastSentMessage.ContainsKey(channel)) LastSentMessage.Add(channel, message);
        else if (LastSentMessage[channel] == message)
        {
            message += "󠀀";
        } 
        MainClient.Client.SendMessage(channel, message);
        LastSentMessage[channel] = message;
    }
    public static void SendColoredMessage(string channel, string message, ChatColor color)
    {
        if (CurrentColor != color)
        {
            SendMessage(Config.Auth.Username, $"/color {color}");
            CurrentColor = color;
        }
        SendMessage(channel, "/me " + message);
    }
    public static async Task SendDiscordMessage(ulong guildID, ulong channelID, string message)
    {
        SocketTextChannel channel = ObjectCache.Get<SocketTextChannel>(channelID + "_DISCORD_CHANNEL")
            ?? DiscordClient.Client.GetGuild(guildID).GetTextChannel(channelID);
        ObjectCache.Put(channelID + "_DISCORD_CHANNEL", channel, 36400);

        await channel.SendMessageAsync(message);
    }
    private static void OnMessageSent(object? sender, OnMessageSentArgs e)
    {
        Log.Debug($"Sent message: {e.SentMessage.Message}");
    }
    private static void OnMessageThrottled(object? sender, OnMessageThrottledEventArgs e)
    {
        Log.Warning($"Message throttled: {e.Message} ({e.SentMessageCount} sent in {e.AllowedInPeriod})");
    }
    #endregion

    #region Receiving
    internal static Task OnDiscordMessageReceived(SocketMessage arg)
    {
        Log.Verbose($"Discord message received => {arg.Author.Username} {arg.Channel.Name}: {arg.Content}");
        HandleDiscordMessage(arg);
        return Task.CompletedTask;
    }
    private static async void OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        Log.Verbose($"#{e.ChatMessage.Channel} {e.ChatMessage.Username}: {e.ChatMessage.Message}");
        await HandleIrcMessage(e.ChatMessage);
    }
    #endregion

    #region Handling
    private static async ValueTask HandleIrcMessage(ChatMessage ircMessage)
    {
        try
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
            if (Regexes.Mention.IsMatch(message))
            {
                string msg = $"`[{DateTime.Now.ToLocalTime():F}] #{ircMessage.Channel} {ircMessage.Username}:` {ircMessage.Message}";
                await SendDiscordMessage(Config.Discord.GuildID, Config.Discord.PingsChannelID, msg);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Handling message failed.");
        }
    }

    private static void HandleDiscordMessage(SocketMessage socketMessage)
    {
        string content = socketMessage.CleanContent;
        ulong channelID = socketMessage.Channel.Id;
        string author = socketMessage.Author.Username.StripDescriminator();
        var embeds = socketMessage.Embeds;

        if (content.Length < 10
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
            content += $" [+{embedCount} embed(s)]";
        }

        var evs = DiscordEvents.Where(x => x.ChannelID == channelID && author.Contains(x.NameContains)).ToArray();
        foreach (var ev in evs)
        {
            string newContent = content.StripSymbols();
            if (ev is null) return;

            if (ev.Remove is not null) newContent = ev.Remove == "_ALL_" ? string.Empty : newContent.Replace(ev.Remove, string.Empty);
            if (ev.Prepend is not null) newContent = $"{ev.Prepend} {newContent}";
            ChatColor color;
            if (Enum.TryParse<ChatColor>(ev.Color, out var clr)) color = clr;
            else color = ChatColor.BlueViolet;

            SendColoredMessage(ev.OutputChannel, newContent, color);
        }
    }
    #endregion
}

internal record DiscordEvent(ulong ChannelID, string NameContains, string? Remove, string OutputChannel, string? Prepend, string Color);
internal enum ChatColor
{
    FANCY_NOT_SET_STATE_NAME,
    Blue, BlueViolet, CadetBlue,
    Chocolate, Coral, DodgerBlue,
    Firebrick, GoldenRod, Green,
    HotPink, OrangeRed, Red,
    SeaGreen, SpringGreen, YellowGreen
}
