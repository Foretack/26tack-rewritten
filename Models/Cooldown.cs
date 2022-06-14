using Serilog;

namespace Tack.Models;
public class Cooldown
{
    private static readonly List<Cooldown> UserCooldownPool = new List<Cooldown>();
    private static readonly List<Cooldown> ChannelCooldownPool = new List<Cooldown>();

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
        int uCD = cd.CooldownOptions.Cooldowns[0];
        int cCD = cd.CooldownOptions.Cooldowns[1];

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

        // I sure hope playing with lists across multiple threads wont cause any trouble !!
        Timer? uTimer = null;
        Timer? cTimer = null;

        // Remove USER cooldown
        uTimer = new Timer(state =>
        {
            UserCooldownPool.Remove(cd);
            Log.Verbose($"- [{cd.User};{cd.CooldownOptions.Name}]");
            uTimer?.Dispose();
        }, null, uCD * 1000, Timeout.Infinite);

        // Remove CHANNEL cooldown
        cTimer = new Timer(state =>
        {
            ChannelCooldownPool.Remove(cd);
            Log.Verbose($"- [{cd.Channel};{cd.CooldownOptions.Name}]");
            cTimer?.Dispose();
        }, null, cCD * 1000, Timeout.Infinite);
        
        // Command can't be executed
        return true;
    } 

    private static void RegisterNewCooldown(Cooldown newCooldown, bool user = true, bool channel = true)
    {
        if (user) UserCooldownPool.Add(newCooldown);
        if (channel) ChannelCooldownPool.Add(newCooldown);
    }
}

public interface ICooldownOptions
{
    public string Name { get; set; }
    public int[] Cooldowns { get; set; }
}
