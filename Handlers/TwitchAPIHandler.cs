﻿using Tack.Nonclass;
using TwitchLib.Api;
using TwitchLib.Api.Core;

namespace Tack.Handlers;
internal sealed class TwitchApiHandler
{
    public static TwitchApiHandler Instance { get; } = new(AppConfigLoader.Config);
    public TwitchAPI Api { get; init; }
    public HttpClient CreateClient => new()
    {
        DefaultRequestHeaders =
        {
            { "Authorization", $"Bearer {AppConfigLoader.Config.BotAccessToken}" },
            {"Client-Id", AppConfigLoader.Config.BotClientId }
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
