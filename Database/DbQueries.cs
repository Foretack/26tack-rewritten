using Dasync.Collections;
using Serilog;
using SqlKata.Execution;
using Tack.Handlers;
using Tack.Models;
using Tack.Utils;

namespace Tack.Database;
internal sealed class DbQueries : DbConnection
{
    public DbQueries(int logLevel = 1) : base(logLevel) { }

    public static DbQueries NewInstance()
    {
        return new DbQueries();
    }

    public async Task<bool> LogException(Exception exception)
    {
        int inserted = await base["errors"].InsertAsync(new
        {
            data = exception.FormatException()
        });

        return inserted > 0;
    }

    public async Task<bool> AddChannel(ExtendedChannel channel)
    {
        int inserted = await base["channels"].InsertAsync(new
        {
            display_name = channel.Displayname,
            username = channel.Username,
            id = int.Parse(channel.ID),
            avatar_url = channel.AvatarUrl,
            priority = channel.Priority,
            is_logged = channel.Logged
        });

        return inserted > 0;
    }

    public async Task<ExtendedChannel[]> GetChannels()
    {
        var query = await base["channels"]
            .Where("priority", ">", -10)
            .OrderByDesc("priority")
            .GetAsync();

        var channels = query.Select(
            x => new ExtendedChannel(
                x.display_name,
                x.username,
                ((int)x.id).ToString(),
                x.avatar_url,
                x.date_joined,
                x.priority,
                x.is_logged
                )
            ).ToArray();
        if (channels is null || channels.Length == 0)
        {
            Log.Fatal("Failed to fetch channel list");
            throw new MissingFieldException("Channel list could not be loaded");
        }

        return channels;
    }

    public async Task<string[]> GetWhitelistedUsers()
    {
        var query = await base["whitelisted_users"]
            .Select("username")
            .GetAsync();

        return query.Select(x => (string)x.username).ToArray();
    }

    public async Task<string[]> GetBlacklistedUsers()
    {
        var query = await base["blacklisted_users"]
            .Select("username")
            .GetAsync();

        return query.Select(x => (string)x.username).ToArray();
    }

    public async Task<Authorization> GetAuthorizationData()
    {
        var query = await base["auth"]
            .GetAsync();

        var row = query.First();
        var auth = new Authorization(row.username, row.access_token, row.client_id, row.supibot_token, row.discord_token);

        return auth;
    }

    public async Task<Discord> GetDiscordData()
    {
        var query = await base["discord"]
            .GetAsync();

        var row = query.First();
        var data = new Discord((ulong)row.guild_id, (ulong)row.pings_channelid, (ulong)row.news_channelid, row.ping_string);

        return data;
    }

    public async Task<bool> RemoveChannel(ExtendedChannel channel)
    {
        int deleted = await base["channels"]
            .Where("username", channel.Username)
            .DeleteAsync();

        return deleted > 0;
    }

    public async Task<bool> CreateSuggestion(PartialUser user, string suggestionText)
    {
        int inserted = await base["suggestions"].InsertAsync(new
        {
            username = user.Username,
            user_id = int.Parse(user.ID),
            suggestion_text = suggestionText
        });

        return inserted > 0;
    }

    public async Task<bool> BlacklistUser(string username, string id)
    {
        int inserted = await base["blacklisted_users"].InsertAsync(new
        {
            username = username,
            id = int.Parse(id)
        });

        return inserted > 0;
    }

    public async Task<bool> WhitelistUser(string username)
    {
        int inserted = await base["blacklisted_users"].InsertAsync(new
        {
            username = username
        });

        return inserted > 0;
    }

    public DiscordEvent[] GetDiscordEvents()
    {
        var query = base["discord_triggers"]
            .Get();

        var events = query.Select(
            x => new DiscordEvent(
                (ulong)x.channel_id,
                x.name_contains,
                x.remove_text,
                x.output_channel,
                x.prepend_text,
                x.color
                )
            ).ToArray();

        return events;
    }

    public async Task<ExtendedChannel?> GetExtendedChannel(string channel)
    {
        var query = await base["channels"]
            .Where("username", channel)
            .GetAsync();

        var row = query.FirstOrDefault();
        if (row is null)
        {
            Log.Error($"Could not get extended channel \"{channel}\"");
            return null;
        }
        var extd = new ExtendedChannel(
            row.display_name,
            row.username,
            ((int)row.id).ToString(),
            row.avatar_url,
            row.date_joined,
            row.priority,
            row.is_logged
            );

        return extd;
    }

    ~DbQueries() => Dispose();
}
