using System.Text;
using System.Text.RegularExpressions;
using AsyncAwaitBestPractices;
using Tack.Core;
using Tack.Database;
using Tack.Models;
using Tack.Utils;
using TwitchLib.Api.Helix.Models.Chat;
using TwitchLib.Client.Events;
using TwitchLib.Communication.Events;

namespace Tack.Handlers;
internal static class MessageHandler
{
    #region Fields
    private static UserColors _currentColor = UserColors.Blue;
    private static DiscordTrigger[] _discordEvents = DbQueries.NewInstance().GetDiscordTriggers().GetAwaiter().GetResult();
    private static readonly HttpClient _requests = TwitchApiHandler.Instance.CreateClient;
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
    public static async Task SendColoredMessage(string channel, string message, UserColors color)
    {
        if (_currentColor != color)
        {
            // TODO: enable this once it's fixed
            //await TwitchAPIHandler.Instance.Api.Helix.Chat.UpdateUserChatColorAsync(MainClient.Self.Id, color);
            HttpResponseMessage response = await _requests.PutAsync(
                $"https://api.twitch.tv/helix/chat/color?user_id={MainClient.Self.Id}&color={color.Value}", null);
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning($"Failed to change user color: {(int)response.StatusCode} {response.StatusCode}");
            }

            _currentColor = color;
        }

        SendMessage(channel, "/me " + message);
    }
    private static void OnMessageSent(object? sender, OnMessageSentArgs e)
    {
        Log.Debug("Sent message: {message}", e.SentMessage.Message);
    }
    private static void OnMessageThrottled(object? sender, OnMessageThrottledEventArgs e)
    {
        Log.Warning("Message throttled: {message} ({count} sent in {period})",
            e.Message,
            e.SentMessageCount,
            e.AllowedInPeriod);
    }
    #endregion

    #region Receiving
    internal static void OnDiscordMessageReceived(object? sender, OnDiscordMsgArgs args)
    {
        DiscordMessage message = args.DiscordMessage;
        Log.Verbose("[{header}] {username} {channel}: {content}",
            $"Discord:{message.GuildName}",
            message.Author.Username,
            message.ChannelName,
            message.Content);
        HandleDiscordMessage(message).SafeFireAndForget(onException: ex => Log.Error(ex, "Error processing Discord message: "));
    }
    private static void OnMessage(object? sender, OnMessageArgs e)
    {
        HandleIrcMessage(e.ChatMessage);
    }
    #endregion

    #region Handling
    private static void HandleIrcMessage(TwitchMessage ircMessage)
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
                CommandHandler.HandleCommand(ctx);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Handling message failed.");
        }
    }

    private static async ValueTask HandleDiscordMessage(DiscordMessage msg)
    {
        DiscordTrigger[] evs = _discordEvents.Where(x =>
            x.ChannelId == msg.ChannelId && (msg.Author.Username.Contains(x.NameContains) || x.NameContains == "_ANY_")
        ).ToArray();
        if (evs.Length == 0)
            return;
        foreach (DiscordTrigger ev in evs)
        {
            bool hasEmbed = msg.Embeds?.Any() ?? false;
            Embed? embed = hasEmbed ? msg.Embeds![0] : null;

            bool hasAttachments = msg.Attachments?.Any(x => !string.IsNullOrEmpty(x.Url) && !string.IsNullOrWhiteSpace(x.Url)) ?? false;
            IEnumerable<string>? attachmentLinks = hasAttachments ? msg.Attachments?.Select(x => x.Url) : null;

            StringBuilder sb = new();
            _ = sb
                .Append(msg.Content)
                .Append(' ')
                .AppendWhen(hasEmbed, $"[{embed?.Title}] ")
                .AppendWhen(hasEmbed && !string.IsNullOrEmpty(embed?.Url), $"( {embed?.Url} ) ")
                .AppendWhen(hasAttachments && attachmentLinks is not null, attachmentLinks?.Join(" 🔗 ")!);

            string m = sb.ToString();

            if (ev.UseRegex && ev.HasGroupReplacements)
            {
                int i = 0;
                foreach (GroupCollection groups in ev.ReplacementRegex.Matches(m).Cast<Match>().Select(m => m.Groups))
                {
                    if (!ev.RegexGroupReplacements.ContainsKey(i))
                        continue;
                    m = m.Replace(groups[i].Value, ev.RegexGroupReplacements[i]);
                    i++;
                }
            }

            // Remove operation
            if (!string.IsNullOrEmpty(ev.RemoveText) && ev.RemoveText != "_ALL_")
                m = m.Replace(ev.RemoveText, "");
            else if (!string.IsNullOrEmpty(ev.RemoveText) && ev.RemoveText == "_ALL_")
                m = string.Empty;

            // Prepend operation
            m = $"{ev.PrependText} " + m;

            // Message is split by 2 new lines
            string[] sMessage = m.Split("\n\n");

            // Strip formatting symbols & show newlines
            sMessage = sMessage.Select(x => x.StripSymbols().Replace("\n", " {⤶} ")).ToArray();

            Queue<string> messages = new();
            foreach (string message in sMessage)
            {
                // Split message into chunks if length is >= 475
                if (message.Length >= 475)
                {
                    IEnumerable<char[]> chunks = message.Chunk(475);
                    foreach (char[] chunk in chunks)
                        messages.Enqueue(new string(chunk) + " [500 LIMIT]");
                    continue;
                }

                messages.Enqueue(message);
            }

            // Send messages every 2.5 seconds
            while (messages.Count > 0)
            {
                await SendColoredMessage(ev.OutChannel, messages.Dequeue(), UserColors.BlueVoilet);
                await Task.Delay(2500);
            }
        }
    }
    #endregion
}
