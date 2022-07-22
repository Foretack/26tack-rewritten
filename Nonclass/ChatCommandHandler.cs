using Tack.Models;

namespace Tack.Nonclass;
public abstract class ChatCommandHandler : ICooldownOptions
{
    public Dictionary<string[], Command> Commands { get; } = new Dictionary<string[], Command>();
    public abstract string Name { get; }
    public abstract string Prefix { get; }
    public virtual bool UseUnifiedCooldowns { get; } = false;
    public virtual short UserCooldown { get; } = 15;
    public virtual short ChannelCooldown { get; } = 5;
    public virtual PermissionLevels Visibility { get; } = PermissionLevels.Everyone;

    protected void AddCommand(Command command)
    {
        List<string> keys = new List<string>(command.Info.Aliases)
        {
            command.Info.Name
        };
        Commands.Add(keys.ToArray(), command);
    }
}