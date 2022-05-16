using Serilog;

namespace _26tack_rewritten.models;
public class Cooldown
{
    private static readonly List<Cooldown> UserCooldownPool = new List<Cooldown>(); // TODO: This is terrible, maybe use a dict?
    private static readonly List<Cooldown> ChannelCooldownPool = new List<Cooldown>();

    public string User { get; set; }
    public string Channel { get; set; }
    public ICooldownOptions CooldownOptions { get; set; }
    private long LastUsed { get; set; }

    public Cooldown(string user, string channel, ICooldownOptions cooldownOptions)
    {
        User = user;
        Channel = channel;
        CooldownOptions = cooldownOptions;
        LastUsed = DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    /// <returns>True if the command can be executed. False if the command is on cooldown</returns>
    public static bool CheckAndHandleCooldown(Cooldown cd)
    {
        int uCD = cd.CooldownOptions.Cooldowns[0];
        int cCD = cd.CooldownOptions.Cooldowns[1];
        long currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();

        if (!UserCooldownPool.Any(x => x.User == cd.User)
        && !ChannelCooldownPool.Any(y => y.Channel == cd.Channel))
        {
            RegisterNewCooldown(cd);
            return true;
        }
        if (UserCooldownPool.Any(x => x.User == cd.User && currentTime - x.LastUsed >= uCD)
        && ChannelCooldownPool.Any(y => y.Channel == cd.Channel && currentTime - y.LastUsed >= cCD))
        {
            UpdateExpiredCooldown(cd, true, true);
            return true;
        }
        if (!UserCooldownPool.Any(x => x.User == cd.User)
        && ChannelCooldownPool.Any(y => y.Channel == cd.Channel && currentTime - y.LastUsed >= cCD))
        {
            UpdateExpiredCooldown(cd, user: false);
            RegisterNewCooldown(cd, channel: false);
            return true;
        }
        if (UserCooldownPool.Any(x => x.User == cd.User && currentTime - x.LastUsed >= uCD)
        && !ChannelCooldownPool.Any(y => y.Channel == cd.Channel))
        {
            UpdateExpiredCooldown(cd, channel: false);
            RegisterNewCooldown(cd, user: false);
            return true;
        }

        Log.Information($"{cd.User} tried using the command \"{cd.CooldownOptions.Name}\" while on cooldown!");
        return false;
    }

    private static void RegisterNewCooldown(Cooldown newCooldown, bool user = true, bool channel = true)
    {
        if (user) UserCooldownPool.Add(newCooldown);
        if (channel) ChannelCooldownPool.Add(newCooldown);
    }
    private static void UpdateExpiredCooldown(Cooldown newCooldown, bool user = true, bool channel = true)
    {
        if (user)
        {
            UserCooldownPool.Remove(UserCooldownPool.First(x => x.User == newCooldown.User));
            UserCooldownPool.Add(newCooldown);
        }
        if (channel)
        {
            ChannelCooldownPool.Remove(UserCooldownPool.First(x => x.Channel == newCooldown.Channel));
            ChannelCooldownPool.Add(newCooldown);
        }
    }
}

public interface ICooldownOptions
{
    public string Name { get; set; }
    public int[] Cooldowns { get; set; }
}
