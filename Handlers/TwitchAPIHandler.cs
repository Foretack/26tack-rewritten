using Tack.Nonclass;
using TwitchLib.Api;
using TwitchLib.Api.Core;

namespace Tack.Handlers;
internal sealed class TwitchApiHandler
{
    public static TwitchApiHandler Instance { get; } = new(AppConfig);
    public TwitchAPI Api { get; init; }
    public HttpClient CreateClient => new()
    {
        DefaultRequestHeaders =
        {
            { "Authorization", $"Bearer {AppConfig.BotAccessToken}" },
            {"Client-Id", AppConfig.BotClientId }
        }
    };

    private TwitchApiHandler(IAppConfig config)
    {
        Api = new TwitchAPI(settings: new ApiSettings()
        {
            AccessToken = config.BotAccessToken,
            ClientId = config.BotClientId,
        });
    }
}
