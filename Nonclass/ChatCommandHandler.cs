using Tack.Models;

namespace Tack.Nonclass;
public abstract class ChatCommandHandler : ICooldownOptions
{
    public Dictionary<string[], IChatCommand> Commands { get; } = new Dictionary<string[], IChatCommand>();
    public string Prefix { get; protected set; } = Config.MainPrefix;
    public bool UseUnifiedCooldowns { get; protected set; } = false;
    public string Name { get; set; } = default!;
    public int[] Cooldowns { get; set; } = default!;
    public PermissionLevels Visibility { get; protected set; } = PermissionLevels.EveryonePlusBlacklisted;

    protected void AddCommand(IChatCommand command)
    {
        List<string> keys = new List<string>(command.Info().Aliases)
        {
            command.Info().Name
        };
        Commands.Add(keys.ToArray(), command);
    }
}