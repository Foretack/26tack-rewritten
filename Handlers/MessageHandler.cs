using System.Text;
using System.Text.RegularExpressions;
using AsyncAwaitBestPractices;
using MiniTwitch.Irc;
using MiniTwitch.Irc.Models;
using Tack.Core;
using Tack.Database;
using Tack.Models;
using Tack.Utils;
using TwitchLib.Api.Helix.Models.Chat;

namespace Tack.Handlers;
public sealed class MessageHandler
{
    #region Fields
    private static DiscordTrigger[] _discordEvents = SingleOf<DbQueries>.Obj.GetDiscordTriggers().GetAwaiter().GetResult();
    private static readonly IrcClient _anonClient = SingleOf<AnonymousClient>.Obj.Client;
    private static readonly IrcClient _mainClient = SingleOf<MainClient>.Obj.Client;
    private static readonly HttpClient _requests = TwitchApiHandler.Instance.CreateClient;
    private static readonly Dictionary<string, string> _lastSentMessage = new();
    private static UserColors _currentColor = UserColors.Blue;
    #endregion

    #region Events
    public static event EventHandler<OnDiscordMsgArgs> OnDiscordMsg
    {
        add => DiscordChat.DiscordMessageManager.AddEventHandler(value, nameof(OnDiscordMsg));
        remove => DiscordChat.DiscordMessageManager.RemoveEventHandler(value, nameof(OnDiscordMsg));
    }
    #endregion

    #region Initialization
    public MessageHandler()
    {
        _mainClient.OnMessage += HandleIrcMessage;
        _anonClient.OnMessage += HandleIrcMessage;
        OnDiscordMsg += OnDiscordMessageReceived;
    }

    public static async Task ReloadDiscordTriggers()
    {
        _discordEvents = await SingleOf<DbQueries>.Obj.GetDiscordTriggers();
    }
    #endregion

    #region Sending
    public static async ValueTask SendMessage(string channel, string message, bool colored = false)
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

        await _mainClient.SendMessage(channel, message, colored);
        _lastSentMessage[channel] = message;
    }
    public static async Task SendColoredMessage(string channel, string message, UserColors color)
    {
        if (_currentColor != color)
        {
            // TODO: enable this once it's fixed
            //await TwitchAPIHandler.Instance.Api.Helix.Chat.UpdateUserChatColorAsync(MainClient.Self.Id, color);
            HttpResponseMessage response = await _requests.PutAsync(
                $"https://api.twitch.tv/helix/chat/color?user_id={SingleOf<MainClient>.Obj.Self.Id}&color={color.Value}", null);
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning($"Failed to change user color: {(int)response.StatusCode} {response.StatusCode}");
            }

            _currentColor = color;
        }

        await SendMessage(channel, message, true);
    }
    #endregion

    #region Receiving
    internal void OnDiscordMessageReceived(object? sender, OnDiscordMsgArgs args)
    {
        DiscordMessage message = args.DiscordMessage;
        Log.Verbose("[{header}] {username} {channel}: {content}",
            $"Discord:{message.GuildName}",
            message.Author.Username,
            message.ChannelName,
            message.Content);
        HandleDiscordMessage(message).SafeFireAndForget(onException: ex => Log.Error(ex, "Error processing Discord message: "));
    }
    #endregion

    #region Handling
    private static ValueTask HandleIrcMessage(Privmsg ircMessage)
    {
        try
        {
            string message = ircMessage.Content;
            string channel = ircMessage.Channel.Name;

            if (CommandHandler.Prefixes.Any(x => message.StartsWith(x))
            && ChannelHandler.MainJoinedChannelNames.Contains(channel))
            {
                string[] splitMessage = message.Replace("\U000E0000", " ").Split(' ');
                string[] commandArgs = splitMessage.Skip(1).ToArray();
                string commandName = splitMessage[0].Replace(CommandHandler.Prefixes.First(x => message.StartsWith(x)), string.Empty);
                var permission = new Permission(ircMessage);
                var ctx = new CommandContext(ircMessage, commandArgs, commandName, permission);
                return CommandHandler.HandleCommand(ctx);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Handling message failed.");
        }

        return ValueTask.CompletedTask;
    }

    private async ValueTask HandleDiscordMessage(DiscordMessage msg)
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
