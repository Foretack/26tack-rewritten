using Tack.Commands.BaseSet;
using Tack.Interfaces;

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
