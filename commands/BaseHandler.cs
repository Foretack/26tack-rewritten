using _26tack_rewritten.commands.baseset;
using _26tack_rewritten.interfaces;

namespace _26tack_rewritten.commands;
internal class BaseHandler : ChatCommandHandler
{
    public BaseHandler()
    {
        SetName = "Base";

        AddCommand(new Ping());
        AddCommand(new RandomJoke());
    }
}
