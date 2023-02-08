using Tack.Handlers;
using Tack.Models;

namespace Tack.Nonclass;

public abstract class Command
{
    public abstract CommandInfo Info { get; }
    public abstract Task Execute(CommandContext ctx);
}