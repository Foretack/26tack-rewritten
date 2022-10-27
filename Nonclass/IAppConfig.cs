namespace Tack.Nonclass;
public interface IAppConfig
{
    string DbHost { get; }
    string DbUser { get; }
    string DbPass { get; }
    string DbName { get; }

    string RedisHost { get; }
    string RedisPass { get; }

    string RelayChannel { get; }
    string BotUsername { get; }
    string BotAccessToken { get; }
    string BotClientId { get; }
    string SupibotApiToken { get; }
    string MainPrefix { get; }

    string LoggingWebhookUrl { get; }

    Dictionary<string, ulong> DiscordChannelIds { get; }
}
