using System.Text;
using Discord;
using Discord.WebSocket;
using Serilog;
using Tack.Handlers;
using Tack.Utils;

namespace Tack.Core;
internal static class DiscordClient
{
    #region Properties
    public static bool Connected { get; private set; } = false;
    public static DiscordSocketClient Client { get; private set; } = default!;

    private static bool OnCooldown { get; set; } = false;
    private static readonly DiscordSocketConfig _config = new()
    {
        GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.GuildPresences | GatewayIntents.AllUnprivileged
    };
    private static readonly Dictionary<byte, string> _rpcData = new();
    #endregion

    #region Initialization
    public static async Task Connect()
    {
        Client = new DiscordSocketClient(_config);
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

    private static async Task PresenceUpdated(SocketUser _user, SocketPresence arg2, SocketPresence arg3)
    {
        if (OnCooldown) return;
        if (_user.IsBot) return;

        await Task.Run(() =>
        {
            var user = (SocketGuildUser)_user;
            var activity = arg2.Activities.FirstOrDefault(x => x.Type != ActivityType.CustomStatus);
            if (activity is null) return;
            var type = (byte)activity.Type;
            if (!_rpcData.ContainsKey(type)) _rpcData.Add(type, string.Empty);

            var sb = new StringBuilder(user.DisplayName);
            switch (activity.Type)
            {
                case ActivityType.Listening:
                    if (activity is SpotifyGame sSong)
                    {
                        if (_rpcData[type] == $"{user.DisplayName}:{sSong.TrackTitle}") return;

                        sb.Append($" is listening to: \"{sSong.TrackTitle}\" by {string.Join(", ", sSong.Artists)}");
                        _rpcData[type] = $"{user.DisplayName}:{sSong.TrackTitle}";
                        break;
                    }
                    return;
                case ActivityType.Streaming:
                    if (activity is StreamingGame sGame)
                    {
                        if (_rpcData[type] == $"{user.DisplayName}:{sGame.Name}") return;

                        sb.Append($" is streaming: {sGame.Name} ({sGame.Url})");
                        _rpcData[type] = $"{user.DisplayName}:{sGame.Name}";
                        break;
                    }
                    return;
                default:
                    if (activity is RichGame game)
                    {
                        if (_rpcData[type] == $"{user.DisplayName}:{game.Name}:{game.Details}:{game.State}") return;

                        sb.Append($" is playing: {game.Name} | {game.Details} | {game.State}");
                        _rpcData[type] = $"{user.DisplayName}:{game.Name}:{game.Details}:{game.State}";
                        break;
                    }
                    break;
            }
            if (sb.ToString() == user.DisplayName) return;

            MessageHandler.SendMessage(Config.RelayChannel, sb.ToString());
        });

        OnCooldown = true;
        Time.Schedule(() => { OnCooldown = false; }, TimeSpan.FromSeconds(120));
    }
    #endregion
}
