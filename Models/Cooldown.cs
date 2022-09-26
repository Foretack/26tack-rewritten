using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Models;
public sealed class Cooldown
{
    private static readonly List<Cooldown> UserCooldownPool = new();
    private static readonly List<Cooldown> ChannelCooldownPool = new();

    public string User { get; set; }
    public string Channel { get; set; }
    public ICooldownOptions CooldownOptions { get; set; }

    public Cooldown(string user, string channel, ICooldownOptions cooldownOptions)
    {
        User = user;
        Channel = channel;
        CooldownOptions = cooldownOptions;
    }

    /// <returns>True if the command can be executed. False if the command is on cooldown</returns>
    public static bool CheckAndHandleCooldown(Cooldown cd)
    {
        int uCD = cd.CooldownOptions.UserCooldown;
        int cCD = cd.CooldownOptions.ChannelCooldown;

        if (uCD == 0 && cCD == 0) return true;

        if (UserCooldownPool.Any(x => x.User == cd.User && x.CooldownOptions.Name == cd.CooldownOptions.Name) // same user + same command
        || ChannelCooldownPool.Any(x => x.Channel == cd.Channel && x.CooldownOptions.Name == cd.CooldownOptions.Name)) // same channel + same command
        {
            Log.Verbose($"[{cd.User};{cd.Channel};{cd.CooldownOptions.Name}] is on cooldown");
            return false;
        }

        // Add cooldowns to their respective lists
        RegisterNewCooldown(cd);
        Log.Verbose($"+ [{cd.User};{cd.Channel};{cd.CooldownOptions.Name}]");

        // Remove USER cooldown
        Time.Schedule(() =>
        {
            _ = UserCooldownPool.Remove(cd);
            Log.Verbose($"- [{cd.User};{cd.CooldownOptions.Name}]");
        }, TimeSpan.FromSeconds(uCD));

        // Remove CHANNEL cooldown
        Time.Schedule(() =>
        {
            _ = ChannelCooldownPool.Remove(cd);
            Log.Verbose($"- [{cd.Channel};{cd.CooldownOptions.Name}]");
        }, TimeSpan.FromSeconds(cCD));

        // Command can't be executed
        return true;
    }

    private static void RegisterNewCooldown(Cooldown newCooldown, bool user = true, bool channel = true)
    {
        if (user) UserCooldownPool.Add(newCooldown);
        if (channel) ChannelCooldownPool.Add(newCooldown);
    }
}


