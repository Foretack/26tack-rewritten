using Microsoft.Extensions.Logging;
using MiniTwitch.Irc;
using MiniTwitch.Irc.Enums;
using Tack.Models;

namespace Tack.Core;

internal sealed class MainClient
{
    public IrcClient Client { get; }
    public User Self { get; private set; } = default!;

    public MainClient()
    {
        Client = new(options =>
        {
            options.Username = AppConfigLoader.Config.BotUsername;
            options.OAuth = AppConfigLoader.Config.BotAccessToken;
            options.Logger = new LoggerFactory().AddSerilog(Log.Logger);
            options.SkipCommandProcessing = SkipCommand.USERNOTICE
                                            | SkipCommand.CLEARCHAT
                                            | SkipCommand.CLEARMSG
                                            | SkipCommand.WHISPER;
        });
    }

    public async Task SetSelf()
    {
        Handlers.Result<User> userResult = await User.Get(AppConfigLoader.Config.BotUsername);
        while (!userResult.Success)
        {
            Log.Fatal("[{header}] Fetching user failed. Retrying...", nameof(MainClient));
            await Task.Delay(1000);
            userResult = await User.Get(AppConfigLoader.Config.BotUsername);
        }

        Self = userResult.Value;
    }
}
