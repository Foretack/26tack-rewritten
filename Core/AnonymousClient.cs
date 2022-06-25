using Serilog;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;
using C = Tack.Core.Core;

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
        Client.OnConnected += ClientConnectedEvent;
        Client.OnDisconnected += ClientDisconnectedEvent;
        Client.OnError += ClientOnErrorEvent;
        Client.OnConnectionError += ClientConnectionErrorEvent;
    }

    private static void ClientConnectedEvent(object? sender, OnConnectedArgs e)
    {
        Log.Information("[Anon] Connected");
        Connected = true;
    }

    private static void ClientConnectionErrorEvent(object? sender, OnConnectionErrorArgs e)
    {
        C.RestartProcess(nameof(ClientConnectionErrorEvent));
    }

    private static void ClientOnErrorEvent(object? sender, OnErrorEventArgs e)
    {
        C.RestartProcess(nameof(ClientOnErrorEvent));
    }

    private static void ClientDisconnectedEvent(object? sender, OnDisconnectedEventArgs e)
    {
        C.RestartProcess(nameof(ClientDisconnectedEvent));
    }
}
