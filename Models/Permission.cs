using Tack.Database;
using Tack.Nonclass;
using TwitchLib.Client.Models;

namespace Tack.Models;
public class Permission
{
    public int Level { get; set; } // forsenLevel
    public string Username { get; set; }

    private bool IsBroadcaster { get; set; }
    private bool IsModerator { get; set; }
    private bool IsVIP { get; set; }
    private bool IsSubscriber { get; set; }

    private static readonly List<string> BlacklistedUsers = DbQueries.NewInstance().GetBlacklistedUsers().Result.ToList();
    private static readonly List<string> WhitelistedUsers = DbQueries.NewInstance().GetWhitelistedUsers().Result.ToList();

    public Permission(ChatMessage ircMessage)
    {
        Username = ircMessage.Username;
        IsBroadcaster = ircMessage.IsBroadcaster;
        IsModerator = ircMessage.IsModerator;
        IsVIP = ircMessage.IsVip;
        IsSubscriber = ircMessage.IsSubscriber;
        Level = EvaluateLevel();
    }

    private int EvaluateLevel()
    {
        int level = 0;

        if (WhitelistedUsers.Contains(Username)) { level = (int)PermissionLevels.Whitelisted; return level; }
        if (BlacklistedUsers.Contains(Username)) { level = (int)PermissionLevels.EveryonePlusBlacklisted; return level; }
        if (IsBroadcaster) { level = (int)PermissionLevels.Broadcaster; return level; }
        if (IsModerator) { level = (int)PermissionLevels.Moderator; return level; }
        if (IsVIP) { level = (int)PermissionLevels.VIP; return level; }
        if (IsSubscriber) { level = (int)PermissionLevels.Subscriber; return level; }

        return level;
    }

    public bool Permits(Command command) => Level >= (int)command.Info.Permission;

    public static void BlacklistUser(string username) { BlacklistedUsers.Add(username); }
    public static void UnBlacklistUser(string username) { BlacklistedUsers.Remove(username); }
    public static void WhitelistUser(string username) { WhitelistedUsers.Add(username); }
    public static void UnWhitelistUser(string username) { WhitelistedUsers.Remove(username); }
}

public enum PermissionLevels
{
    EveryonePlusBlacklisted = -10,
    Everyone = 0,
    Subscriber = 1,
    VIP = 2,
    Moderator = 3,
    Broadcaster = 4,
    Whitelisted = 5
}
