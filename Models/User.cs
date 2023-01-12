using Tack.Database;
using Tack.Handlers;
using Tl = TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace Tack.Models;
internal sealed class UserFactory
{
    public async Task<User?> CreateUserAsync(string username)
    {
        var userCache = await Redis.Cache.TryGetObjectAsync<User>($"twitch:users:{username}");
        if (!userCache.keyExists)
        {
            var r = await TwitchAPIHandler.GetUsers(username);
            if (r is null)
            {
                var r2 = await ExternalAPIHandler.GetIvrUser(username);
                if (r2 is null) return default!;
                userCache.value = new(r2.DisplayName, r2.Login, r2.Id.ToString(), r2.Logo, r2.CreatedAt ?? DateTime.MinValue);
            }
            else
                userCache.value = new User(r.DisplayName, r.Login, r.Id, r.ProfileImageUrl, r.CreatedAt);
        }
        return userCache.value;
    }
    public async Task<User[]?> CreateUserAsync(params string[] usernames)
    {
        List<string> nonCachedUsernames = new();
        List<User> users = new();
        foreach (var username in usernames)
        {
            var userCache = await Redis.Cache.TryGetObjectAsync<User>($"twitch:users:{username}");
            if (!userCache.keyExists)
            {
                nonCachedUsernames.Add(username);
                continue;
            }
            users.Add(userCache.value);
        }
        if (nonCachedUsernames.Count > 0)
        {
            var r = await TwitchAPIHandler.GetUsers(nonCachedUsernames.ToArray());
            if (r is null) return default!;
            foreach (var user in r
            .Where(x => x is not null)
            .Select(x => new User(x.DisplayName, x.Login, x.Id, x.ProfileImageUrl, x.CreatedAt)))
            {
                await Redis.Cache.SetObjectAsync($"twitch:users:{user.Username}", user, TimeSpan.FromDays(1));
                users.Add(user);
            }
        }

        return users.ToArray();
    }
    public async Task<User?> CreateUserByIDAsync(string id)
    {
        Tl::User? call = await TwitchAPIHandler.GetUsersByID(id);
        if (call is null) return null;
        var u = new User(call.DisplayName, call.Login, call.Id, call.ProfileImageUrl, call.CreatedAt);
        return u;
    }
    public async Task<User[]?> CreateUserByIDAsync(params string[] ids)
    {
        Tl::User[]? call = await TwitchAPIHandler.GetUsersByID(ids);
        if (call is null || call.Length == 0) return null;
        var users = new List<User>();
        foreach (Tl::User u in call)
        {
            try
            {
                var user = new User(u.DisplayName, u.Login, u.Id, u.ProfileImageUrl, u.CreatedAt);
                users.Add(user);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to enumerate over an array of users");
            }
        }
        return users.ToArray();
    }
    public async Task<ExtendedChannel?> CreateChannelProfile(ChannelHandler.Channel extender)
    {
        Tl::User? call = await TwitchAPIHandler.GetUsers(extender.Name);
        return call is null
            ? null
            : new ExtendedChannel(call.DisplayName,
                                   call.Login,
                                   call.Id,
                                   call.ProfileImageUrl,
                                   call.CreatedAt,
                                   extender.Priority,
                                   extender.Logged);
    }
}

public sealed record PartialUser(string Displayname, string Username, string ID);
public sealed record User(string Displayname, string Username, string ID, string AvatarUrl, DateTime DateCreated);
public sealed record ExtendedChannel(string Displayname, string Username, string ID, string AvatarUrl, DateTime DateJoined, int Priority, bool Logged);
