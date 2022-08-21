using System.Diagnostics;
using System.Text;
using Discord;
using Discord.WebSocket;
using Serilog;
using Tack.Handlers;

namespace Tack.Core;
internal static class DiscordClient
{
    #region Properties
    public static bool Connected { get; private set; } = false;
    public static DiscordSocketClient Client { get; private set; } = default!;

    private static readonly DiscordSocketConfig ClientConfig = new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildPresences
    };
    #endregion

    #region Initialization
    public static async Task Connect()
    {
        Client = new DiscordSocketClient(ClientConfig);
        await Client.LoginAsync(TokenType.Bot, Config.Auth.DiscordToken);
        await Client.StartAsync();
        RegisterEvents(Client);
    }

    private static void RegisterEvents(DiscordSocketClient client)
    {
        client.MessageReceived += MessageHandler.OnDiscordMessageReceived;
        client.Connected += OnConnected;
        client.Disconnected += OnDisconnected;
        client.PresenceUpdated += PresenceUpdated;
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

    private static async Task PresenceUpdated(SocketUser user, SocketPresence arg2, SocketPresence arg3)
    {
        if (user.IsBot) return;

        await Task.Run(() =>
        {
            var activity = arg2.Activities.FirstOrDefault();

            if (activity is null) return;

            var sb = new StringBuilder(user.Username);
            switch (activity.Type)
            {
                case ActivityType.Listening:
                    if (activity is SpotifyGame sSong)
                    {
                        sb.Append($" is listening to: \"{sSong.TrackTitle}\" by {string.Join(", ", sSong.Artists)}");
                        break;
                    }
                    return;
                case ActivityType.Streaming:
                    if (activity is StreamingGame sGame)
                    {
                        sb.Append($" is streaming: {sGame.Name} ({sGame.Url})");
                        break;
                    }
                    return;
                default:
                    if (activity is RichGame game)
                    {
                        sb.Append($" is playing: {game.Name} | {game.Details} | {game.State}");
                        break;
                    }
                    sb.Append($" is playing: {activity.Name} " + (string.IsNullOrEmpty(activity.Details) ? String.Empty : $"({activity.Details})"));
                    break;
            }

            MessageHandler.SendMessage(Config.RelayChannel, sb.ToString());
        });
    }
    #endregion
}
