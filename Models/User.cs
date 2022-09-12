using Serilog;
using Tack.Handlers;
using Tack.Utils;
using Tl = TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace Tack.Models;
internal sealed class UserFactory
{
    public async Task<User?> CreateUserAsync(string username)
    {
        User? c = ObjectCache.Get<User>(username + "_users");
        if (c is not null) return c;
        Tl::User? call = await TwitchAPIHandler.GetUsers(username);
        if (call is null)
        {
            User? call2 = await ExternalAPIHandler.GetIvrUser(username);
            if (call2 is null) return null;
            ObjectCache.Put(username + "_users", call2, 86400);
            return call2;
        }
        var u = new User(call.DisplayName, call.Login, call.Id, call.ProfileImageUrl, call.CreatedAt);
        ObjectCache.Put(username + "_users", u, 86400);
        return u;
    }
    public async Task<User[]?> CreateUserAsync(params string[] usernames)
    {
        Tl::User[]? call = await TwitchAPIHandler.GetUsers(usernames);
        if (call is null || call.Length == 0) return null;
        var users = new List<User>();
        foreach (Tl::User u in call)
        {
            try
            {
                var user = new User(u.DisplayName, u.Login, u.Id, u.ProfileImageUrl, u.CreatedAt);
                users.Add(user);
                ObjectCache.Put(user.Username + "_user", user, 86400);
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
