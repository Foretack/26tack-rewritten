using _26tack_rewritten.database;
using _26tack_rewritten.handlers;
using Serilog;
using Serilog.Core;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace _26tack_rewritten.core;

public static class MainClient
{
    public static LoggingLevelSwitch LogSwitch { get; } = new LoggingLevelSwitch();
    public static DateTime StartupTime { get; private set; } = new DateTime();
    public static bool Running { get; set; } = true;

    internal static TwitchClient Client { get; private set; } = new TwitchClient();

    private static bool Errored { get; set; } = false;

    public static async Task<int> Main(string[] args)
    {
        LogSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Debug;
        Log.Logger = new LoggerConfiguration().MinimumLevel.ControlledBy(LogSwitch).WriteTo.Console().CreateLogger();

        Database db = new Database();
        Config.Auth = await db.GetAuthorizationData();
        Config.Discord = await db.GetDiscordData();
        Config.Links = new Links();

        StartupTime = DateTime.Now;

        if (Running) Initialize();
        while (Running) Console.ReadLine();
        return 0;
    }

    private static void Initialize()
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

        ConnectionCredentials credentials = new ConnectionCredentials(Config.Auth.Username, Config.Auth.AccessToken);
        Client.Initialize(credentials);

        Connect();
    }

    private static void Connect()
    {
        Client.Connect();
        Client.OnConnectionError += (s, e) =>
        {
            Errored = true;
            Log.Fatal($"MainClient encountered a connection error: {e.Error.Message}");
        };
        Client.OnConnected += ClientConnectedEvent;
    }

    private static async void ClientConnectedEvent(object? sender, OnConnectedArgs e)
    {
        Log.Information($"[Main] Connected");
        AnonymousClient.Initialize();
        MessageHandler.Initialize();
        CommandHandler.Initialize();
        await DiscordClient.Connect();
        while (!(AnonymousClient.Connected && DiscordClient.Connected))
        {
            await Task.Delay(1000);
        }
        await ChannelHandler.Connect(false);
        if (!Errored) return;
        await Reconnect();
        Errored = false;
    }

    // TODO: AnonymousClient doesn't have this.
    // Also check which client needs to rejoin the channels properly
    // instead of making them both do it you lazy idiot
    private static async Task Reconnect()
    {
        Client.JoinChannel(Config.RelayChannel);
        Client.SendMessage(Config.RelayChannel, $"ppCircle Reconnecting...");
        Log.Information("MainClient is attempting reconnection...");
        await ChannelHandler.Connect(Errored);
        Errored = false;
    }
}