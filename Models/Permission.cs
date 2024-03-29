﻿using MiniTwitch.Irc.Models;
using Tack.Database;
using Tack.Nonclass;

namespace Tack.Models;

public sealed class Permission
{
    private const string BROADCASTER_ROLE = "broadcaster/1";

    public int Level { get; set; } // forsenLevel
    public string Username { get; set; }

    private readonly bool _isBroadcaster;
    private readonly bool _isModerator;
    private readonly bool _isVIP;
    private readonly bool _isSubscriber;

    private static readonly List<string> _blacklistedUsers = SingleOf<DbQueries>.Obj.GetBlacklistedUsers().Result.ToList();
    private static readonly List<string> _whitelistedUsers = SingleOf<DbQueries>.Obj.GetWhitelistedUsers().Result.ToList();

    public Permission(Privmsg ircMessage)
    {
        Username = ircMessage.Author.Name;
        _isBroadcaster = ircMessage.Author.Badges.Contains(BROADCASTER_ROLE);
        _isModerator = ircMessage.Author.IsMod;
        _isVIP = ircMessage.Author.IsVip;
        _isSubscriber = ircMessage.Author.IsSubscriber;
        Level = EvaluateLevel();
    }

    // TODO: this is terrible
    private int EvaluateLevel()
    {
        int level = 0;

        if (_whitelistedUsers.Contains(Username))
        {
            level = (int)PermissionLevels.Whitelisted;
            return level;
        }

        if (_blacklistedUsers.Contains(Username))
        {
            level = (int)PermissionLevels.EveryonePlusBlacklisted;
            return level;
        }

        if (_isBroadcaster)
        {
            level = (int)PermissionLevels.Broadcaster;
            return level;
        }

        if (_isModerator)
        {
            level = (int)PermissionLevels.Moderator;
            return level;
        }

        if (_isVIP)
        {
            level = (int)PermissionLevels.VIP;
            return level;
        }

        if (_isSubscriber)
        {
            level = (int)PermissionLevels.Subscriber;
            return level;
        }

        return level;
    }

    public bool Permits(Command command)
    {
        return Level >= (int)command.Info.Permission;
    }

    public static bool IsBlacklisted(string username)
    {
        return _blacklistedUsers.Contains(username);
    }

    public static void BlacklistUser(string username) { _blacklistedUsers.Add(username); }
    public static void UnBlacklistUser(string username) { _ = _blacklistedUsers.Remove(username); }
    public static void WhitelistUser(string username) { _whitelistedUsers.Add(username); }
    public static void UnWhitelistUser(string username) { _ = _whitelistedUsers.Remove(username); }
}

public enum PermissionLevels : sbyte
{
    EveryonePlusBlacklisted = -10,
    Everyone = 0,
    Subscriber = 1,
    VIP = 2,
    Moderator = 3,
    Broadcaster = 4,
    Whitelisted = 5
}
