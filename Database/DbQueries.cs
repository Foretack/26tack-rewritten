using Dasync.Collections;
using SqlKata.Execution;
using Tack.Handlers;
using Tack.Models;
using Tack.Utils;

namespace Tack.Database;
internal sealed class DbQueries : DbConnection
{
    public static DbQueries NewInstance()
    {
        return new DbQueries();
    }

    public async Task<bool> LogException(Exception exception)
    {
        int inserted = await Enqueue("errors", q => q.InsertAsync(new
        {
            data = exception.FormatException()
        }));

        return inserted > 0;
    }

    public async Task<bool> AddChannel(ExtendedChannel channel)
    {
        int inserted = await Enqueue("channels", q => q.InsertAsync(new
        {
            display_name = channel.Displayname,
            username = channel.Username,
            id = int.Parse(channel.ID),
            avatar_url = channel.AvatarUrl,
            priority = channel.Priority,
            is_logged = channel.Logged
        }));

        return inserted > 0;
    }

    public async Task<ExtendedChannel[]> GetChannels()
    {
        var query = await Enqueue("channels", q => q
            .Where("priority", ">", -10)
            .OrderByDesc("priority")
            .GetAsync());

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
        var query = await Enqueue("whitelisted_users", q => q.Select("username").GetAsync());

        return query.Select(x => (string)x.username).ToArray();
    }

    public async Task<string[]> GetBlacklistedUsers()
    {
        var query = await Enqueue("blacklisted_users", q => q
            .Select("username")
            .GetAsync());

        return query.Select(x => (string)x.username).ToArray();
    }

    public async Task<bool> RemoveChannel(ExtendedChannel channel)
    {
        int deleted = await Enqueue("channels", q => q
            .Where("username", channel.Username)
            .DeleteAsync());

        return deleted > 0;
    }

    public async Task<bool> CreateSuggestion(PartialUser user, string suggestionText)
    {
        int inserted = await Enqueue("suggestions", q => q.InsertAsync(new
        {
            username = user.Username,
            user_id = int.Parse(user.ID),
            suggestion_text = suggestionText
        }));

        return inserted > 0;
    }

    public async Task<bool> BlacklistUser(string username, string id)
    {
        int inserted = await Enqueue("blacklisted_users", q => q.InsertAsync(new
        {
            username = username,
            id = int.Parse(id)
        }));

        return inserted > 0;
    }

    public async Task<bool> WhitelistUser(string username)
    {
        int inserted = await Enqueue("whitelisted_users", q => q.InsertAsync(new
        {
            username = username
        }));

        return inserted > 0;
    }

    public async Task<DiscordTrigger[]> GetDiscordTriggers()
    {
        var query = await Enqueue("discord_triggers", q => q
            .GetAsync());

        var events = query.Select(x => new DiscordTrigger(x)).ToArray();

        return events;
    }

    public async Task<ExtendedChannel?> GetExtendedChannel(string channel)
    {
        var query = await Enqueue("channels", q => q
            .Where("username", channel)
            .GetAsync());

        var row = query.FirstOrDefault();
        if (row is null)
        {
            Log.Error("Could not get extended channel \"{channel}\"", channel);
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

    public async Task<int> UpdateUsers(int[] ids)
    {
        var users = await ExternalAPIHandler.GetIvrUsersById(ids);
        Log.Debug("Fetched {c} users from Ivr", users.Length);

        int updated = 0;
        foreach (var user in users)
        {
            if (user.Banned && user.BanReason == "TOS_INDEFINITE")
            {
                updated += await Enqueue($"UPDATE twitch_users SET banned = true WHERE id = {user.Id}");
            }

            int u = await Enqueue($"UPDATE twitch_users SET account = ROW('{user.DisplayName}', '{user.Login}', {user.Id}, '{user.Logo}', DATE '{user.CreatedAt ?? DateTime.MinValue}', CURRENT_DATE), inserted = true WHERE id = {user.Id}", 2500);
            Log.Verbose("User updated: {u}#{i}", user.Login, user.Id);
            await Task.Delay(250);
        }

        Log.Debug("Finished updating users; {c} total updates", updated);
        return updated;
    }

    ~DbQueries() => Dispose();
}
