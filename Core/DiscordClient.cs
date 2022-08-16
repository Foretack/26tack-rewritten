using Discord;
using Discord.WebSocket;
using Serilog;
using Tack.Handlers;

namespace Tack.Core;
internal static class DiscordClient
{
    #region Properties
    public static bool Connected { get; private set; } = false;
    public static DiscordSocketClient Client { get; } = new DiscordSocketClient();
    #endregion

    #region Initialization
    public static async Task Connect()
    {
        await Client.LoginAsync(TokenType.Bot, Config.Auth.DiscordToken);
        await Client.StartAsync();
        RegisterEvents(Client);
    }

    private static void RegisterEvents(DiscordSocketClient client)
    {
        client.MessageReceived += MessageHandler.OnDiscordMessageReceived;
        client.Connected += OnConnected;
        client.Disconnected += OnDisconnected;
    }
    #endregion

    #region Client events
    private static Task OnDisconnected(Exception arg)
    {
        Log.Warning("[Discord] Disconnected");
        Connected = false;
        return Task.CompletedTask;
    }

    private static Task OnConnected()
    {
        Log.Information("[Discord] Connected");
        Connected = true;
        return Task.CompletedTask;
    }
    #endregion
}
