using Tack.Commands.BaseSet;
using Tack.Nonclass;

namespace Tack.Commands;
internal class BaseHandler : ChatCommandHandler
{
    public override string Name => "Base";
    public override string Prefix => Config.MainPrefix;

    public BaseHandler()
    {
        AddCommand(new Ping());
        AddCommand(new RandomJoke());
        AddCommand(new Suggest());
        AddCommand(new LongPing());
    }
}
