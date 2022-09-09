using Dasync.Collections;
using Serilog;
using SqlKata.Execution;
using Tack.Handlers;
using Tack.Models;
using Tack.Utils;

namespace Tack.Database;
internal class DbQueries : DbConnection
{
    public static DbQueries NewInstance()
    {
        return new DbQueries();
    }

    public async Task<bool> LogException(Exception exception)
    {
        int inserted = await QueryFactory.Query("errors").InsertAsync(new
        {
            data = exception.FormatException()
        });

        return inserted > 0;
    }

    public async Task<bool> AddChannel(ExtendedChannel channel)
    {
        int inserted = await QueryFactory.Query("channels").InsertAsync(new
        {
            display_name = channel.Displayname,
            username = channel.Username,
            id = int.Parse(channel.ID),
            avatar_url = channel.AvatarUrl,
            priority = channel.Priority,
            is_logged = channel.Logged,
            date_joined = DateTime.Now
        });

        return inserted > 0;
    }

    public async Task<ChannelHandler.Channel[]> GetChannels()
    {
        var query = await QueryFactory.Query("channels")
            .Where("priority", ">", -10)
            .Select("username", "priority", "is_logged")
            .OrderByDesc("priority")
            .GetAsync();

        var channels = query.Select(
            x => new ChannelHandler.Channel(x.username, x.priority, x.is_logged)
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
        var query = await QueryFactory.Query("whitelisted_users")
            .Select("username")
            .GetAsync();

        return query.Select(x => (string)x.username).ToArray();
    }

    public async Task<string[]> GetBlacklistedUsers()
    {
        var query = await QueryFactory.Query("blacklisted_users")
            .Select("username")
            .GetAsync();

        return query.Select(x => (string)x.username).ToArray();
    }

    public async Task<Authorization> GetAuthorizationData()
    {
        var query = await QueryFactory.Query("auth")
            .GetAsync();

        var row = query.First();
        var auth = new Authorization(row.username, row.access_token, row.client_id, row.supibot_token, row.discord_token);

        return auth;
    }

    public async Task<Discord> GetDiscordData()
    {
        var query = await QueryFactory.Query("discord")
            .GetAsync();

        var row = query.First();
        var data = new Discord((ulong)row.guild_id, (ulong)row.pings_channelid, (ulong)row.news_channelid, row.ping_string);

        return data;
    }

    public async Task<bool> RemoveChannel(ChannelHandler.Channel channel)
    {
        int deleted = await QueryFactory.Query("channels")
            .Where("username", channel.Name)
            .DeleteAsync();

        return deleted > 0;
    }

    public async Task<bool> CreateSuggestion(PartialUser user, string suggestionText)
    {
        int inserted = await QueryFactory.Query("suggestions").InsertAsync(new
        {
            username = user.Username,
            user_id = int.Parse(user.ID),
            suggestion_text = suggestionText
        });

        return inserted > 0;
    }

    public async Task<bool> BlacklistUser(string username, string id)
    {
        int inserted = await QueryFactory.Query("blacklisted_users").InsertAsync(new
        {
            username = username,
            id = int.Parse(id)
        });

        return inserted > 0;
    }

    public async Task<bool> WhitelistUser(string username)
    {
        int inserted = await QueryFactory.Query("blacklisted_users").InsertAsync(new
        {
            username = username
        });

        return inserted > 0;
    }

    public DiscordEvent[] GetDiscordEvents()
    {
        var query = QueryFactory.Query("discord_triggers")
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
        var query = await QueryFactory.Query("channels")
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
