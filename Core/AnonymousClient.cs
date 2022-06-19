using Serilog;
using Tack.Handlers;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.Communication.Events;

namespace Tack.Core;
internal static class AnonymousClient
{
    public static bool Connected { get; set; } = false;

    internal static TwitchClient Client { get; private set; } = new TwitchClient();
    public static void Initialize()
    {
        ClientOptions options = new ClientOptions();
        options.MessagesAllowedInPeriod = 1;
        options.ThrottlingPeriod = TimeSpan.FromSeconds(1);

        ReconnectionPolicy policy = new ReconnectionPolicy(10);
        policy.SetMaxAttempts(10);
        options.ReconnectionPolicy = policy;

        WebSocketClient webSocketClient = new WebSocketClient(options);
        Client = new TwitchClient(webSocketClient);
        Client.AutoReListenOnException = true;

        ConnectionCredentials credentials = new ConnectionCredentials("justinfan123", "justinfan");
        Client.Initialize(credentials, Config.Auth.Username);

        Connect();
    }

    public static void Connect()
    {
        Client.Connect();
        Client.OnConnectionError += (s, e) 
            => Log.Warning($"AnonymousClient encountered a connection error: {e.Error.Message}");
        Client.OnConnected += ClientConnectedEvent;
        Client.OnDisconnected += ClientDisconnectedEvent;
        Client.OnReconnected += async (s, e) 
            => await Reconnect();
    }

    private static void ClientConnectedEvent(object? sender, OnConnectedArgs e)
    {
        Log.Information("[Anon] Connected");
        Connected = true;
    }

    private static void ClientDisconnectedEvent(object? sender, OnDisconnectedEventArgs e)
    {
        Log.Warning($"[Anon] Disconnected");
        Connected = false;
        Client.Reconnect();
    }

    private static async Task Reconnect()
    {
        Client.JoinChannel(Config.RelayChannel);
        Log.Information("AnonymousClient is attempting reconnection...");
        Log.Information($"AnonymousClient has triggered {typeof(ChannelHandler)} once more!");
        await ChannelHandler.Connect(true);
    }
}
