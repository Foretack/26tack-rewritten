using Tack.Commands.BaseSet;
using Tack.Nonclass;

namespace Tack.Commands;
internal sealed class BaseHandler : ChatCommandHandler
{
    public override string Name => "Base";
    public override string Prefix => AppConfigLoader.Config.BasePrefix;

    public BaseHandler()
    {
        AddCommand(new Ping());
        AddCommand(new RandomJoke());
        AddCommand(new Suggest());
        //AddCommand(new LongPing()); Note: Don't uncomment until bot is verified. This causes issues with ratelimits
        AddCommand(new RandomLink());
        AddCommand(new RandomMidjourney());
    }
}
