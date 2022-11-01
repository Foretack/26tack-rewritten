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
    #region Fields
    private static ChatColor _currentColor = ChatColor.FANCY_NOT_SET_STATE_NAME;
    private static DiscordTrigger[] _discordEvents = DbQueries.NewInstance().GetDiscordTriggers().GetAwaiter().GetResult();
    private static readonly Dictionary<string, string> _lastSentMessage = new();
    #endregion

    #region Events
    public static event EventHandler<OnDiscordMsgArgs> OnDiscordMsg
    {
        add => DiscordChat.DiscordMessageManager.AddEventHandler(value, nameof(OnDiscordMsg));
        remove => DiscordChat.DiscordMessageManager.RemoveEventHandler(value, nameof(OnDiscordMsg));
    }
    public static event EventHandler<OnMessageArgs> OnTwitchMsg
    {
        add => AnonymousChat.TwitchMessageManager.AddEventHandler(value, nameof(OnTwitchMsg));
        remove => AnonymousChat.TwitchMessageManager.RemoveEventHandler(value, nameof(OnTwitchMsg));
    }
    #endregion

    #region Initialization
    public static void Initialize()
    {
        OnTwitchMsg += OnMessage;
        OnDiscordMsg += OnDiscordMessageReceived;
        MainClient.Client.OnMessageSent += OnMessageSent;
        MainClient.Client.OnMessageThrottled += OnMessageThrottled;

        Time.DoEvery(TimeSpan.FromHours(6), async () => await ReloadDiscordTriggers());
    }

    public static async Task ReloadDiscordTriggers()
    {
        using var db = new DbQueries();
        _discordEvents = await db.GetDiscordTriggers();
    }
    #endregion

    #region Sending
    public static void SendMessage(string channel, string message)
    {
        message = message.Length >= 500 ? message[..495] + "..." : message;
        if (!_lastSentMessage.ContainsKey(channel))
        {
            _lastSentMessage.Add(channel, message);
        }
        else if (_lastSentMessage[channel] == message)
        {
            message += " 󠀀";
        }
        MainClient.Client.SendMessage(channel, message);
        _lastSentMessage[channel] = message;
    }
    public static void SendColoredMessage(string channel, string message, ChatColor color)
    {
        if (_currentColor != color)
        {
            SendMessage(AppConfigLoader.Config.BotUsername, $"/color {color}");
            _currentColor = color;
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
    internal static void OnDiscordMessageReceived(object? sender, OnDiscordMsgArgs args)
    {
        var message = args.DiscordMessage;
        Log.Verbose($"Discord message received => {message.Author.Username} {message.ChannelName}: {message.Content}");
        HandleDiscordMessage(message).SafeFireAndForget(onException: ex => Log.Error(ex, $"Error processing Discord message: "));
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
        DiscordTrigger[] evs = _discordEvents.Where(
            x => x.ChannelId == msg.ChannelId
            && (msg.Author.Username.Contains(x.NameContains)
            || x.NameContains == "_ANY_")
            ).ToArray();
        if (evs.Length == 0) return;
        foreach (var ev in evs)
        {
            if (!string.IsNullOrEmpty(ev.RemoveText) && ev.RemoveText.Contains(" _SHOW_ALL_"))
            {
                // Remove "_SHOW_ALL_" to properly .Remove()
                string newRemove = ev.RemoveText.Replace(" _SHOW_ALL_", "");
                // Get links of all attachments in message
                IEnumerable<string> attachmentLinks = msg.Attachments.Select(x => x.Url + ' ');
                // Message clean content + attachment links joined with 🔗
                string m = $"{msg.Content} \n\n" + string.Join(" 🔗 ", attachmentLinks);
                // Remove operation
                if (!string.IsNullOrEmpty(newRemove)) m = m.Replace(newRemove, "");
                // Prepend operation
                m = $"{ev.PrependText} " + m;
                // Message is split by 2 new lines
                string[] sMessage = m.Split("\n\n");
                // Strip formatting symbols & show newlines
                sMessage = sMessage.Select(x => x.StripSymbols().Replace("\n", "[⤶] ")).ToArray();

                var messages = new Queue<string>();
                foreach (string message in sMessage)
                {
                    // Split message into chunks if length is >= 475
                    if (message.Length >= 475)
                    {
                        IEnumerable<char[]> chunks = message.Chunk(475);
                        foreach (char[] chunk in chunks) messages.Enqueue(new string(chunk) + " [500 LIMIT]");
                        continue;
                    }
                    messages.Enqueue(message);
                }

                // Send messages every 2.5 seconds
                while (messages.Count > 0)
                {
                    SendMessage(ev.OutChannel, messages.Dequeue());
                    await Task.Delay(2500);
                }
                continue;
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

            if (ev.RemoveText is not null) content = ev.RemoveText == "_ALL_" ? string.Empty : content.Replace(ev.RemoveText, string.Empty);
            if (ev.PrependText is not null) content = $"{ev.PrependText} {content}";
            SendMessage(ev.OutChannel, content);
        }
    }
    #endregion
}

internal enum ChatColor
{
    FANCY_NOT_SET_STATE_NAME,
    Blue, BlueViolet, CadetBlue,
    Chocolate, Coral, DodgerBlue,
    Firebrick, GoldenRod, Green,
    HotPink, OrangeRed, Red,
    SeaGreen, SpringGreen, YellowGreen
}
