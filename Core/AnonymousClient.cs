using Microsoft.Extensions.Logging;
using MiniTwitch.Irc;
using Tack.Models;

namespace Tack.Core;
internal sealed class AnonymousClient : Singleton
{
    public IrcClient Client { get; }
    public int Reconnects { get; private set; } = 0;

    public AnonymousClient()
    {
        Client = new(options =>
        {
            options.Anonymous = true;
            options.Logger = new LoggerFactory().AddSerilog(Log.Logger).CreateLogger<AnonymousClient>();
        })
        {
            ReconnectionDelay = TimeSpan.FromMinutes(1)
        };

        Client.OnReconnect += () =>
        {
            Reconnects++;
            return ValueTask.CompletedTask;
        };
    }
}
