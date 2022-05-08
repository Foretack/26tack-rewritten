using System.Text.Json;
using _26tack_rewritten.json;
using Serilog;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using _26tack_rewritten.handlers;

namespace _26tack_rewritten.core;

public static class MainClient
{
    public static List<string> JLChannels { get; private set; } = new List<string>();

    internal static TwitchClient Client { get; private set; } = new TwitchClient();

    private static bool Running { get; set; } = true;
    private static bool Errored { get; set; } = false;
    private static readonly HttpClient HttpClient = new HttpClient();

    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        
        if (Running) await Initialize();
        while (Running) Console.Read();
        return 0;
    }

    private static async Task Initialize()
    {
        ClientOptions options = new ClientOptions();
        options.MessagesAllowedInPeriod = 150;
        options.ThrottlingPeriod = TimeSpan.FromSeconds(30);

        ReconnectionPolicy policy = new ReconnectionPolicy(10);
        policy.SetMaxAttempts(10);
        options.ReconnectionPolicy = policy;

        WebSocketClient webSocketClient = new WebSocketClient(options);
        Client = new TwitchClient(webSocketClient);
        Client.AutoReListenOnException = true;

        ConnectionCredentials credentials = new ConnectionCredentials(Config.Username, Config.AccessToken);
        Client.Initialize(credentials);

        Stream jlcl = await HttpClient.GetStreamAsync(Config.JLChannelListLink);
        JustLogLoggedChannels deserialized = (await JsonSerializer.DeserializeAsync<JustLogLoggedChannels>(jlcl))!;
        JLChannels = deserialized.channels.Select(c => c.name).ToList();

        await Connect();
    }

    private static async Task Connect()
    {
        Client.Connect();
        Client.OnConnectionError += (s, e) => Errored = true;
        Client.OnConnected += ClientConnectedEvent;
        await ChannelHandler.Connect(Errored);
    }

    private static async void ClientConnectedEvent(object? sender, OnConnectedArgs e)
    {
        if (!Errored) return;
        await Reconnect();
    }

    private static async Task Reconnect()
    {
        Client.JoinChannel(Config.RelayChannel);
        Client.SendMessage(Config.RelayChannel, $"ppCircle Reconnecting...");
        await ChannelHandler.Connect(Errored);
        Errored = false;
    }
}