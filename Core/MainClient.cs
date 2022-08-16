using Serilog;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;
using C = Tack.Core.Core;

namespace Tack.Core;
public static class MainClient
{
    #region Properties
    public static bool Connected { get; private set; } = false;
    public static TwitchClient Client { get; private set; } = new TwitchClient();
    #endregion

    #region Initialization
    public static void Initialize()
    {
        var options = new ClientOptions();
        options.MessagesAllowedInPeriod = 150;
        options.ThrottlingPeriod = TimeSpan.FromSeconds(30);

        var policy = new ReconnectionPolicy(10);
        policy.SetMaxAttempts(10);
        options.ReconnectionPolicy = policy;

        var webSocketClient = new WebSocketClient(options);
        Client = new TwitchClient(webSocketClient);
        Client.AutoReListenOnException = true;

        var credentials = new ConnectionCredentials(Config.Auth.Username, Config.Auth.AccessToken);
        Client.Initialize(credentials);

        Connect();
    }

    private static void Connect()
    {
        _ = Client.Connect();
        Client.OnConnected += ClientConnectedEvent;
        Client.OnDisconnected += ClientDisconnectedEvent;
        Client.OnError += ClientErrorEvent;
        Client.OnConnectionError += ClientConnectionErrorEvent;
    }
    #endregion

    #region Client events
    private static void ClientConnectedEvent(object? sender, OnConnectedArgs e)
    {
        Log.Information($"[Main] Connected");
        Connected = true;
    }

    private static void ClientConnectionErrorEvent(object? sender, OnConnectionErrorArgs e)
    {
        C.RestartProcess(nameof(ClientConnectionErrorEvent));
    }

    private static void ClientErrorEvent(object? sender, OnErrorEventArgs e)
    {
        C.RestartProcess(nameof(ClientErrorEvent));
    }

    private static void ClientDisconnectedEvent(object? sender, OnDisconnectedEventArgs e)
    {
        C.RestartProcess(nameof(ClientDisconnectedEvent));
    }
    #endregion
}