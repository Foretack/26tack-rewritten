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
            await MessageHandler.SendDiscordMessage(Config.Discord.GuildID,
                                                    Config.Discord.PingsChannelID,
                                                    $"[{DateTime.Now.ToLocalTime()}] {Config.Discord.DiscordPing} an exception failed to be logged!");
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
            .Values($"'{channel.Displayname}'", $"'{channel.Username}'", $"{channel.ID}", $"'{channel.AvatarUrl}'", $"{channel.Priority}", $"{channel.logged}", "CURRENT_DATE")
            .TryExecute();

        return q.Success;
    }

    public async Task<ChannelHandler.Channel[]> GetChannels()
    {
        var q = await
            Select()
            .Table("channels")
            .Schema("username", "priority", "is_logged")
            .Sort("priority DESC")
            .TryExecute();

        if (!q.Success)
        {
            Log.Fatal("Failed to fetch channel list");
            throw new MissingFieldException("Channel list could not be loaded");
        }

        List<ChannelHandler.Channel> channels = new List<ChannelHandler.Channel>();
        foreach (object[] row in q.Results!)
        {
            channels.Add(new ChannelHandler.Channel((string)row[0], (int)row[1], (bool)row[2]));
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

    public async Task<Authorization> GetAuthorizationData()
    {
        var q = await
            Select()
            .Table("auth")
            .Schema("*")
            .TryExecute();

        if (!q.Success)
        {
            Log.Fatal("Failed to fetch authorization data from the database");
            throw new MissingFieldException("Failed to fetch authorization data from the database");
        }

        try
        {
            return new Authorization((string)q.Results![0][0],
                                     (string)q.Results![0][1],
                                     (string)q.Results![0][2],
                                     (string)q.Results![0][3],
                                     (string)q.Results![0][4]);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "There was an error processing authorization data");
            throw;
        }
    }

    public async Task<Discord> GetDiscordData()
    {
        var q = await
            Select()
            .Table("discord")
            .Schema("*")
            .TryExecute();

        if (!q.Success)
        {
            Log.Fatal("Failed to fetch discord data from the database");
            throw new MissingFieldException("Failed to fetch discord data from the database");
        }

        try
        {
            return new Discord(ulong.Parse(q.Results![0][0].ToString()!),
                               ulong.Parse(q.Results![0][1].ToString()!),
                               ulong.Parse(q.Results![0][2].ToString()!),
                               (string)q.Results![0][3]);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "There was an error processing discord data");
            throw;
        }
    }

    public async Task<bool> RemoveChannel(ChannelHandler.Channel channel)
    {
        var q = await
            Delete()
            .Table("channels")
            .Where($"username = '{channel.Name}'")
            .TryExecute();

        return q.Success;
    }

    public async Task<bool> CreateSuggestion(PartialUser user, string suggestionText)
    {
        var q = await
            Insert()
            .Table("suggestions")
            .Schema("username", "user_id", "suggestion_text")
            .Values($"'{user.Username}'", $"{user.ID}", $"'{suggestionText}'")
            .TryExecute();

        return q.Success;
    }
}
