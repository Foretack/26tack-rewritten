using Microsoft.Extensions.Logging;
using MiniTwitch.Irc;
using Tack.Models;

namespace Tack.Core;
internal sealed class AnonymousClient : Singleton
{
    public IrcClient Client { get; }

    public AnonymousClient()
    {
        Client = new(options =>
        {
            options.Anonymous = true;
            options.Logger = new LoggerFactory().AddSerilog(Log.Logger);
        });
    }
}
