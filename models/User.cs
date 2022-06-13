using _26tack_rewritten.handlers;
using _26tack_rewritten.utils;
using Serilog;

namespace _26tack_rewritten.models;
internal class UserFactory
{
    public async Task<User?> CreateUserAsync(string username)
    {
        var c = ObjectCaching.GetCachedObject<User>(username + "_users");
        if (c is not null) return c;
        var call = await TwitchAPIHandler.GetUsers(username);
        if (call is null)
        {
            var call2 = await ExternalAPIHandler.GetIvrUser(username);
            if (call2 is null) return null;
            ObjectCaching.CacheObject(username + "_users", call2, 86400);
            return call2;
        }
        User u = new User(call.DisplayName, call.Login, call.Id, call.ProfileImageUrl, call.CreatedAt);
        ObjectCaching.CacheObject(username + "_users", u, 86400);
        return u;
    }
    public async Task<User[]?> CreateUserAsync(params string[] usernames)
    {
        var call = await TwitchAPIHandler.GetUsers(usernames);
        if (call is null || call.Length == 0) return null;
        List<User> users = new List<User>();
        foreach (var u in call)
        {
            try
            {
                User user = new User(u.DisplayName, u.Login, u.Id, u.ProfileImageUrl, u.CreatedAt);
                users.Add(user);
                ObjectCaching.CacheObject(user.Username + "_user", user, 86400);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to enumerate over a user in an array of users");
            }
        }
        return users.ToArray();
    }
    public async Task<User?> CreateUserByIDAsync(string id)
    {
        var call = await TwitchAPIHandler.GetUsersByID(id);
        if (call is null) return null;
        User u = new User(call.DisplayName, call.Login, call.Id, call.ProfileImageUrl, call.CreatedAt);
        return u;
    }
    public async Task<User[]?> CreateUserByIDAsync(params string[] ids)
    {
        var call = await TwitchAPIHandler.GetUsersByID(ids);
        if (call is null || call.Length == 0) return null;
        List<User> users = new List<User>();
        foreach (var u in call)
        {
            try
            {
                User user = new User(u.DisplayName, u.Login, u.Id, u.ProfileImageUrl, u.CreatedAt);
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
        var call = await TwitchAPIHandler.GetUsers(extender.Name);
        if (call is null) return null;
        return new ExtendedChannel(call.DisplayName,
                                   call.Login,
                                   call.Id,
                                   call.ProfileImageUrl,
                                   call.CreatedAt,
                                   extender.Priority,
                                   extender.Logged);
    }
}

public record PartialUser(string Displayname, string Username, string ID);
public record User(string Displayname, string Username, string ID, string AvatarUrl, DateTime DateCreated);
public record ExtendedChannel(string Displayname, string Username, string ID, string AvatarUrl, DateTime DateCreated, int Priority, bool logged);
