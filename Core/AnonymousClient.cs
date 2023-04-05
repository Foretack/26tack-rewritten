using Microsoft.Extensions.Logging;
using MiniTwitch.Irc;
using MiniTwitch.Irc.Enums;
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
            options.SkipCommandProcessing = SkipCommand.USERNOTICE
                                            | SkipCommand.CLEARCHAT
                                            | SkipCommand.CLEARMSG
                                            | SkipCommand.WHISPER;
        });
    }
}
