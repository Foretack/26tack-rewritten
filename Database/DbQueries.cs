using System;
using SqlKata.Execution;
using Tack.Handlers;
using Tack.Models;
using Tack.Utils;

namespace Tack.Database;
internal sealed class DbQueries : DbConnection
{
    public async Task<bool> LogException(Exception exception)
    {
        int inserted = await ValueStatement(async qf =>
        {
            return await qf.Query("errors").InsertAsync(new
            {
                data = exception.FormatException()
            });
        });

        return inserted > 0;
    }

    public async Task<bool> AddChannel(ExtendedChannel channel)
    {
        int inserted = await ValueStatement(async qf =>
        {
            return await qf.Query("channels").InsertAsync(new
            {
                display_name = channel.Displayname,
                username = channel.Username,
                id = channel.Id,
                avatar_url = channel.AvatarUrl,
                priority = channel.Priority,
                is_logged = channel.Logged
            });
        });

        return inserted > 0;
    }

    public async Task<ExtendedChannel[]> GetChannels()
    {
        IEnumerable<dynamic> query = await ValueStatement(async qf => await qf.Query("channels")
            .Where("priority", ">", -10)
            .OrderByDesc("priority")
            .GetAsync());

        ExtendedChannel[]? channels = query.Select(
            x => new ExtendedChannel(
                x.display_name,
                x.username,
                (long)x.id,
                x.avatar_url,
                x.date_joined,
                x.priority,
                x.is_logged
                )
            ).ToArray();
        if (channels is not { Length: > 0 })
        {
            Log.Fatal("Failed to fetch channel list");
            throw new MissingFieldException("Channel list could not be loaded");
        }

        return channels;
    }

    public async Task<string[]> GetWhitelistedUsers()
    {
        IEnumerable<dynamic> query = await ValueStatement(async qf => await qf.Query("whitelisted_users").Select("username")
        .GetAsync());

        return query.Select(x => (string)x.username).ToArray();
    }

    public async Task<string[]> GetBlacklistedUsers()
    {
        IEnumerable<dynamic> query = await ValueStatement(async qf => await qf.Query("blacklisted_users").Select("username")
            .GetAsync());

        return query.Select(x => (string)x.username).ToArray();
    }

    public void RemoveChannel(ExtendedChannel channel)
    {
        Enqueue(async q => await q.Query("channels")
            .Where("username", channel.Username)
            .DeleteAsync());
    }

    public void CreateSuggestion(PartialUser user, string suggestionText)
    {
        Enqueue(async qf => await qf.Query("suggestions").InsertAsync(new
        {
            username = user.Username,
            user_id = user.Id,
            suggestion_text = suggestionText
        }));
    }

    public void BlacklistUser(string username, long id)
    {
        Enqueue(async q => await q.Query("blacklisted_users").InsertAsync(new
        {
            username,
            id
        }));
    }

    public void WhitelistUser(string username)
    {
        Enqueue(async q => await q.Query("whitelisted_users").InsertAsync(new
        {
            username
        }));
    }

    public async Task<DiscordTrigger[]> GetDiscordTriggers()
    {
        IEnumerable<dynamic> query = await ValueStatement(async q => await q.Query("discord_triggers")
            .GetAsync());
        DiscordTrigger[] events = query.Select(x => new DiscordTrigger(x)).ToArray();
        return events;
    }

    public async Task<ExtendedChannel?> GetExtendedChannel(string channel)
    {
        IEnumerable<dynamic> query = await ValueStatement(async q => await q.Query("channels")
            .Where("username", channel)
            .GetAsync());
        dynamic? row = query.FirstOrDefault();
        if (row is null)
        {
            Log.Error("Could not get extended channel \"{channel}\"", channel);
            return null;
        }

        var extd = new ExtendedChannel(
            row.display_name,
            row.username,
            (long)row.id,
            row.avatar_url,
            row.date_joined,
            row.priority,
            row.is_logged
            );

        return extd;
    }

    public async Task UpdateUsers(int[] ids)
    {
        IvrUser[] users = await ExternalApiHandler.GetIvrUsersById(ids);
        Log.Debug("Fetched {c} users from Ivr", users.Length);
        foreach (IvrUser user in users)
        {
            if (user.Banned && user.BanReason == "TOS_INDEFINITE")
            {
                Enqueue(async qf => await qf.StatementAsync($"UPDATE twitch_users SET banned = true WHERE id = {user.Id}"));
            }

            Enqueue(async qf => await qf.StatementAsync($"UPDATE twitch_users SET account = ROW('{user.DisplayName}', '{user.Login}', {user.Id}, '{user.Logo}', DATE '{user.CreatedAt ?? DateTime.MinValue}', CURRENT_DATE), inserted = true WHERE id = {user.Id}"));
            Log.Verbose("Enqueued user update: {u}#{i}", user.Login, user.Id);
        }

        Log.Debug("[{h}] Finished updating users", nameof(DbQueries));
    }
}
