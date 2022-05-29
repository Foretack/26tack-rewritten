using Serilog;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace _26tack_rewritten.core;
internal static class AnonymousClient
{
    public static bool Connected { get; set; } = false;
    public static TwitchClient Client { get; private set; } = new TwitchClient();


    public static void Initialize()
    {
        ClientOptions options = new ClientOptions();
        options.MessagesAllowedInPeriod = 1;
        options.ThrottlingPeriod = TimeSpan.FromSeconds(1);

        ReconnectionPolicy policy = new ReconnectionPolicy(10);
        policy.SetMaxAttempts(50);
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
        Client.OnConnectionError += (s, e) =>
        {
            Log.Error($"AnonymousClient encountered a connection error: {e.Error.Message}");
        };
        Client.OnConnected += (s, e) =>
        {
            Connected = true;
            Log.Information("[Anon] Connected");
        };
    }
}
