using System.Text.Json;
using _26tack_rewritten.json;
using Serilog;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace _26tack_rewritten.src;

public static class BotClient
{
    public static List<string> JLChannels { get; private set; } = new List<string>();
    public static List<string> JoinedChannels { get; } = new List<string>();

    internal static TwitchClient Client { get; private set; } = new TwitchClient();

    private static bool Running { get; set; } = true;
    private static HttpClient HttpClient { get; } = new HttpClient();

    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        
        await Initialize();
        while (Running)
        {
            Console.Read();
        }
        return -1;
    }

    private static async Task Initialize()
    {
        ClientOptions options = new ClientOptions();
        options.MessagesAllowedInPeriod = 150;
        options.ThrottlingPeriod = TimeSpan.FromSeconds(30);

        ReconnectionPolicy policy = new ReconnectionPolicy();
        policy.SetMaxAttempts(5);
        options.ReconnectionPolicy = policy;

        WebSocketClient webSocketClient = new WebSocketClient(options);
        Client = new TwitchClient(webSocketClient);
        Client.AutoReListenOnException = true;

        ConnectionCredentials credentials = new ConnectionCredentials(Config.Username, Config.AccessToken);
        Client.Initialize(credentials);

        Stream jlcl = await HttpClient.GetStreamAsync(Config.JLChannelListLink);
        JustLogLoggedChannels deserialized = (await JsonSerializer.DeserializeAsync<JustLogLoggedChannels>(jlcl))!;
        JLChannels = deserialized.channels.Select(c => c.name).ToList();
    }

    private static async Task Connect()
    {
        Client.Connect();
        Client.OnConnectionError += ConnectionErrorEvent;
    }

    private static async void ConnectionErrorEvent(object? sender, OnConnectionErrorArgs e)
    {
        throw new NotImplementedException();
    }
}