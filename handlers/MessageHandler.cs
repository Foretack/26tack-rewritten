using _26tack_rewritten.core;
using _26tack_rewritten.misc;
using _26tack_rewritten.models;
using Serilog;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace _26tack_rewritten.handlers;
internal static class MessageHandler
{
    private static ChatColor CurrentColor = ChatColor.FANCY_NOT_SET_STATE_NAME;
    private static readonly HttpClient Requests = new HttpClient();

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
            MainClient.Client.SendMessage(Config.Username, $"/color {color}");
            CurrentColor = color;
        }
        MainClient.Client.SendMessage(channel, message);
    }

    private static void OnMessageThrottled(object? sender, OnMessageThrottledEventArgs e)
    {
        Log.Warning($"Message throttled: {e.Message} ({e.SentMessageCount} sent in {e.AllowedInPeriod})");
    }

    private static void OnMessageSent(object? sender, OnMessageSentArgs e)
    {
        Log.Debug($"Sent message: {e.SentMessage.Message}");
    }

    private static async void OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        await HandleMessage(e.ChatMessage);
    }

    private static async Task HandleMessage(ChatMessage ircMessage)
    {
        string message = ircMessage.Message;
        string channel = ircMessage.Channel;
        string prefix = Config.Prefix;
        string[] splitMessage = message.Split(' ');
        string[] commandArgs = splitMessage.Skip(1).ToArray();

        if (message.StartsWith(prefix))
        {
            string commandName = splitMessage[0].Replace(prefix, string.Empty);
            Permission permission = new Permission(ircMessage);
            CommandContext ctx = new CommandContext(ircMessage, commandArgs, commandName, permission);
            CommandHandler.HandleCommand(ctx);
        }
        if (channel == "pajlada"
        && ircMessage.Username == "pajbot"
        && ircMessage.IsMe
        && message.StartsWith("pajaS🚨ALERT"))
        {
            SendMessage("pajlada", utils.Random.Choice(RandomReplies.PajbotReplies));
        }
        if (Regexes.Mention.IsMatch(message))
        {
            // TODO: Discord JSON stuff
        }
    }
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
