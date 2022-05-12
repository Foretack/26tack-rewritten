namespace _26tack_rewritten.models;

public class Command
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string[] Aliases { get; set; }
    public int[] Cooldowns { get; set; }
    public PermissionLevels Permission { get; set; }

    public Command(string name, string? description = null, string[]? aliases = null, int[]? cooldowns = null, PermissionLevels permission = PermissionLevels.Everyone)
    {
        Name = name;
        Description = description ?? "No Description.";
        Aliases = aliases ?? Array.Empty<string>();
        Cooldowns = cooldowns ?? new int[] { 10, 5 };
        Permission = permission;
    }
}
