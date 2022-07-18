using Tack.Commands.BaseSet;
using Tack.Nonclass;

namespace Tack.Commands;
internal class BaseHandler : ChatCommandHandler
{
    public BaseHandler()
    {
        Name = "Base";

        AddCommand(new Ping());
        AddCommand(new RandomJoke());
        AddCommand(new Suggest());
    }
}
