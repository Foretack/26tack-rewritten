using _26tack_rewritten.handlers;
using _26tack_rewritten.utils;
using Serilog;

namespace _26tack_rewritten.models;
internal class UserFactory : DataCacher<User>
{
    public async Task<User?> CreateUserAsync(string username)
    {
        var c = GetCachedPiece(username);
        if (c is not null) return c.Object;
        var call = await TwitchAPIHandler.GetUsers(username);
        if (call is null) return null;
        User u = new User(call.DisplayName, call.Login, call.Id, call.ProfileImageUrl, call.CreatedAt);
        CachePiece(username, u, 86400);
        CachePiece(u.ID, u, 86400);
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
                CachePiece(user.Username, user, 86400);
                CachePiece(user.ID, user, 86400);
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
        var c = GetCachedPiece(id);
        if (c is not null) return c.Object;
        var call = await TwitchAPIHandler.GetUsersByID(id);
        if (call is null) return null;
        User u = new User(call.DisplayName, call.Login, call.Id, call.ProfileImageUrl, call.CreatedAt);
        CachePiece(id, u, 86400);
        CachePiece(u.Username, u, 86400);
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
                CachePiece(user.Username, user, 86400);
                CachePiece(user.ID, user, 86400);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to enumerate over an array of users");
            }
        }
        return users.ToArray();
    }
    public async Task<ExtendedChannel?> CreateChannelProfile(string channelName)
    {
        ChannelHandler.Channel? extender = ChannelHandler.MainJoinedChannels
            .Concat(ChannelHandler.AnonJoinedChannels)
            .First(x => x.Name == channelName);
        if (extender is null)
        {
            Log.Error("Attempted to extend a nonexisting channel");
            return null;
        }
        var c = GetCachedPiece(channelName);
        if (c is not null)
        {
            User cc = c.Object;
            return new ExtendedChannel(cc.Displayname, cc.Username, cc.ID, cc.AvatarUrl, cc.DateCreated, extender.Priority, extender.Logged);
        }
        var call = await TwitchAPIHandler.GetUsers(channelName);
        if (call is null) return null;
        return new ExtendedChannel(call.DisplayName, call.Login, call.Id, call.ProfileImageUrl, call.CreatedAt, extender.Priority, extender.Logged);
    }
}

public record PartialUser(string Displayname, string Username, string ID);
public record User(string Displayname, string Username, string ID, string AvatarUrl, DateTime DateCreated);
public record ExtendedChannel(string Displayname, string Username, string ID, string AvatarUrl, DateTime DateCreated, int Priority, bool logged);
