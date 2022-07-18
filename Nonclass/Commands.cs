using Tack.Handlers;
using Tack.Models;

namespace Tack.Nonclass;

public interface IChatCommand
{
    public Command Info();
    public Task Run(CommandContext ctx);
}
