using Tack.Nonclass;

namespace Tack.Models;

public class CommandInfo : ICooldownOptions
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string[] Aliases { get; private set; }
    public short UserCooldown { get; private set; }
    public short ChannelCooldown { get; private set; }
    public PermissionLevels Permission { get; private set; }

    public CommandInfo(string name, string? description = null, string[]? aliases = null, 
    short? userCooldown = null, short? channelCooldown = null, PermissionLevels? permission = null)
    {
        Name = name;
        Description = description ?? "No Description";
        Aliases = aliases ?? Array.Empty<string>();
        UserCooldown = userCooldown ?? 15;
        ChannelCooldown = channelCooldown ?? 5;
        Permission = permission ?? PermissionLevels.Everyone;
    }
}
