using _26tack_rewritten.handlers;
using _26tack_rewritten.models;
using _26tack_rewritten.utils;
using Serilog;

namespace _26tack_rewritten.database;
internal class Database : DbConnection
{
    public async Task<bool> LogException(Exception exception)
    {
        var q = await
            Insert()
            .Table("errors")
            .Schema("data", "time")
            .Values($"'{Formatting.FormatException(exception)}'", $"CURRENT_TIMESTAMP")
            .TryExecute();

        if (!q.Success)
        {
            // TODO: Discord message
            return false;
        }
        return true;
    }

    public async Task<bool> AddChannel(ExtendedChannel channel)
    {
        var q = await
            Insert()
            .Table("channels")
            .Schema("display_name", "username", "id", "avatar_url", "priority", "is_logged", "date_joined")
            .Values($"'{channel.Displayname}'", $"'{channel.Username}'", $"'{channel.AvatarUrl}'", $"{channel.Priority}", $"{channel.logged}", "CURRENT_DATE")
            .TryExecute();

        if (!q.Success) return false;
        return true;
    }

    public async Task<ChannelHandler.Channel[]> GetChannels()
    {
        var q = await
            Select()
            .Table("channels")
            .Schema("username", "priority", "is_logged")
            .TryExecute();

        if (!q.Success)
        {
            Log.Fatal("Failed to fetch channel list");
            throw new MissingFieldException("Channel list could not be loaded");
        }

        List<ChannelHandler.Channel> channels = new List<ChannelHandler.Channel>();
        foreach (object[] row in q.Results!)
        {
            channels.Add(new ChannelHandler.Channel((string)row[1], (int)row[4], (bool)row[5]));
        }

        return channels.ToArray();
    }

    public async Task<string[]> GetWhitelistedUsers()
    {
        var q = await
            Select()
            .Table("whitelisted_users")
            .Schema("username")
            .TryExecute();

        if (!q.Success)
        {
            Log.Fatal("Failed to fetch whitelisted users");
            throw new MissingFieldException("Whitelisted users could not be loaded");
        }

        string[] users = q.Results!.Select(x => (string)x[0]).ToArray();
        return users;
    }

    public async Task<string[]> GetBlacklistedUsers()
    {
        var q = await
            Select()
            .Table("blacklisted_users")
            .Schema("username")
            .TryExecute();

        if (!q.Success)
        {
            Log.Fatal("Failed to fetch blacklisted users");
            throw new MissingFieldException("Blacklisted users could not be loaded");
        }

        string[] users = q.Results!.Select(x => (string)x[0]).ToArray();
        return users;
    }
}
