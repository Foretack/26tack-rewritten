using Microsoft.Extensions.Logging;
using MiniTwitch.Irc;
using MiniTwitch.Irc.Interfaces;
using MiniTwitch.Irc.Models;
using Tack.Models;

namespace Tack.Core;

internal sealed class MainClient : Singleton
{
    public IrcClient Client { get; }
    public User Self { get; private set; } = default!;

    private readonly Dictionary<string, IUserstateSelf> _states = new();

    public MainClient()
    {
        Client = new(options =>
        {
            options.Username = AppConfig.BotUsername;
            options.OAuth = AppConfig.BotAccessToken;
            options.Logger = new LoggerFactory().AddSerilog(Log.Logger).CreateLogger<MainClient>();
        });
        Client.OnUserstate += OnUserstate;
    }

    public async Task SetSelf()
    {
        Handlers.Result<User> userResult = await User.Get(AppConfig.BotUsername);
        while (!userResult.Success)
        {
            Log.Fatal("[{header}] Fetching user failed. Retrying...", nameof(MainClient));
            await Task.Delay(1000);
            userResult = await User.Get(AppConfig.BotUsername);
        }

        Self = userResult.Value;
    }

    public bool ModeratesChannel(string channel)
    {
        if (!_states.ContainsKey(channel))
            return false;

        return _states[channel].IsMod;
    }

    private ValueTask OnUserstate(Userstate ustate)
    {
        _states[ustate.Channel.Name] = ustate.Self;
        return ValueTask.CompletedTask;
    }
}
