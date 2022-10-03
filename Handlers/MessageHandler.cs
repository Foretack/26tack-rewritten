using AsyncAwaitBestPractices;
using Tack.Core;
using Tack.Database;
using Tack.Models;
using Tack.Utils;
using TwitchLib.Client.Events;
using TwitchLib.Communication.Events;

namespace Tack.Handlers;
internal static class MessageHandler
{
    #region Properties
    private static ChatColor CurrentColor { get; set; } = ChatColor.FANCY_NOT_SET_STATE_NAME;
    private static DiscordEvent[] DiscordEvents { get; set; } = DbQueries.NewInstance().GetDiscordEvents();
    private static readonly Dictionary<string, string> LastSentMessage = new();
    #endregion

    #region Initialization
    public static void Initialize()
    {
        AnonymousChat.OnMessage += OnMessage;
        MainClient.Client.OnMessageSent += OnMessageSent;
        MainClient.Client.OnMessageThrottled += OnMessageThrottled;
    }

    public static void ReloadDiscordTriggers() { DiscordEvents = DbQueries.NewInstance().GetDiscordEvents(); }
    #endregion

    #region Sending
    public static void SendMessage(string channel, string message)
    {
        message = message.Length >= 500 ? message[..495] + "..." : message;
        if (!LastSentMessage.ContainsKey(channel))
        {
            LastSentMessage.Add(channel, message);
        }
        else if (LastSentMessage[channel] == message)
        {
            message += " 󠀀";
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
    internal static Task OnDiscordMessageReceived(DiscordMessage message)
    {
        Log.Verbose($"Discord message received => {message.Author.Username} {message.ChannelName}: {message.Content}");
        HandleDiscordMessage(message).SafeFireAndForget(onException: ex => Log.Error(ex, $"Error processing Discord message: "));
        return Task.CompletedTask;
    }
    private static async void OnMessage(object? sender, OnMessageArgs e)
    {
        Log.Verbose($"#{e.ChatMessage.Channel} {e.ChatMessage.Username}: {e.ChatMessage.Message}");
        await HandleIrcMessage(e.ChatMessage);
    }
    #endregion

    #region Handling
    private static async ValueTask HandleIrcMessage(TwitchMessage ircMessage)
    {
        try
        {
            string message = ircMessage.Message;
            string channel = ircMessage.Channel;
            string[] splitMessage = message.Replace("\U000E0000", " ").Split(' ');
            string[] commandArgs = splitMessage.Skip(1).ToArray();

            if (CommandHandler.Prefixes.Any(x => message.StartsWith(x))
            && ChannelHandler.MainJoinedChannelNames.Contains(channel))
            {
                string commandName = splitMessage[0].Replace(CommandHandler.Prefixes.First(x => message.StartsWith(x)), string.Empty);
                var permission = new Permission(ircMessage);
                var ctx = new CommandContext(ircMessage, commandArgs, commandName, permission);
                await CommandHandler.HandleCommand(ctx);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Handling message failed.");
        }
    }

    private static async ValueTask HandleDiscordMessage(DiscordMessage msg)
    {
        DiscordEvent[] evs = DiscordEvents.Where(
            x => x.ChannelID == msg.ChannelId
            && (msg.Author.Username.StripDescriminator().Contains(x.NameContains)
            || x.NameContains == "_ANY_")
            ).ToArray();
        if (evs.Length == 0) return;
        foreach (DiscordEvent? ev in evs)
        {
            if (ev.Remove?.Contains(" _SHOW_ALL_") ?? false)
            {
                // Remove "_SHOW_ALL_" to properly .Remove()
                string newRemove = ev.Remove.Replace(" _SHOW_ALL_", "");
                // Get links of all attachments in message
                IEnumerable<string> attachmentLinks = msg.Attachments.Select(x => x.Url + ' ');
                // Message clean content + attachment links joined with 🔗
                string m = $"{msg.Content} \n\n" + string.Join(" 🔗 ", attachmentLinks);
                // Remove operation
                if (!string.IsNullOrEmpty(newRemove)) m = m.Replace(newRemove, "");
                // Prepend operation
                m = $"{ev.Prepend} " + m;
                // Message is split by 2 new lines
                string[] sMessage = m.Split("\n\n");
                // Strip formatting symbols & show newlines
                sMessage = sMessage.Select(x => x.StripSymbols().Replace("\n", "[⤶] ")).ToArray();

                var messages = new Queue<string>();
                foreach (string message in sMessage)
                {
                    // Split message into chunks if length is >= 488
                    if (message.Length >= 488)
                    {
                        IEnumerable<char[]> chunks = message.Chunk(488);
                        foreach (char[] chunk in chunks) messages.Enqueue(new string(chunk) + " [500 LIMIT]");
                        continue;
                    }
                    messages.Enqueue(message);
                }

                // Get color
                ChatColor _color = Enum.TryParse<ChatColor>(ev.Color, out ChatColor _clr) ? _clr : ChatColor.BlueViolet;

                // Send messages every 2.5 seconds
                while (messages.Count > 0)
                {
                    SendColoredMessage(ev.OutputChannel, messages.Dequeue(), _color);
                    await Task.Delay(2500);
                }
                return;
            }
            string content = msg.Content.Replace("\n", " ⤶ ").StripSymbols();
            IReadOnlyCollection<Embed> embeds = msg.Embeds;

            if (content.Length < 50
            && embeds.Count > 0)
            {
                int embedCount = embeds.Count;
                Embed embed = embeds.First();
                content =
                    $"{embed.Title.StripSymbols()} " +
                    $"{(embed.Url is null ? string.Empty : $"( {embed.Url} )")} " +
                    $"{(embedCount > 1 ? $"[+{"embed".PluralizeWith(embedCount - 1)}]" : string.Empty)}";
            }
            else if (content.Length >= 50
            && content.Length <= 450
            && embeds.Count > 0)
            {
                int embedCount = embeds.Count;
                content += $" [+{"Embed".PluralizeWith(embedCount)}]";
            }

            if (ev.Remove is not null) content = ev.Remove == "_ALL_" ? string.Empty : content.Replace(ev.Remove, string.Empty);
            if (ev.Prepend is not null) content = $"{ev.Prepend} {content}";
            ChatColor color = Enum.TryParse<ChatColor>(ev.Color, out ChatColor clr) ? clr : ChatColor.BlueViolet;
            SendColoredMessage(ev.OutputChannel, content, color);
        }
    }
    #endregion
}

internal sealed record DiscordEvent(ulong ChannelID, string NameContains, string? Remove, string OutputChannel, string? Prepend, string Color);
internal enum ChatColor
{
    FANCY_NOT_SET_STATE_NAME,
    Blue, BlueViolet, CadetBlue,
    Chocolate, Coral, DodgerBlue,
    Firebrick, GoldenRod, Green,
    HotPink, OrangeRed, Red,
    SeaGreen, SpringGreen, YellowGreen
}
