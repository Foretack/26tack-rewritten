using Tack.Nonclass;
using TwitchLib.Api;
using TwitchLib.Api.Core;

namespace Tack.Handlers;
internal sealed class TwitchAPIHandler
{
    public static TwitchAPIHandler Instance { get; } = new(AppConfigLoader.Config);
    public TwitchAPI Api { get; init; }

    private TwitchAPIHandler(IAppConfig config)
    {
        Api = new TwitchAPI(settings: new ApiSettings()
        {
            AccessToken = config.BotAccessToken,
            ClientId = config.BotClientId,
        });

        Api.Helix.Settings.AccessToken = config.BotAccessToken;
        Api.Helix.Settings.ClientId = config.BotClientId;
    }
}
