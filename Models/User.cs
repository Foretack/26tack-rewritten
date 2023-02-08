using Tack.Handlers;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using HelixUser = TwitchLib.Api.Helix.Models.Users.GetUsers.User;

namespace Tack.Models;
public sealed class User
{
    public string DisplayName { get; init; }
    public string Username { get; init; }
    public string Id { get; init; }
    public string Type { get; init; }
    public string BroadcasterType { get; init; }
    public DateTime CreatedAt { get; init; }
    public string Description { get; init; }
    public string Email { get; init; }
    public string AvatarUrl { get; init; }
    public string OfflineImageUrl { get; init; }

    public static async Task<Result<User>> Get(string username)
    {
        GetUsersResponse helixUser = await TwitchAPIHandler.Instance.Api.Helix.Users.GetUsersAsync(logins: new() { username });
        HelixUser? user = helixUser.Users.FirstOrDefault();

        if (user is null)
        {
            Log.Error("Failed to get user {user} from Helix", username);
            return new Result<User>(default!, false, new ArgumentOutOfRangeException());
        }

        return new Result<User>(new(user), true, default!);
    }

    public static async Task<Result<ExtendedChannel>> GetChannel(ChannelHandler.Channel @base)
    {
        Result<User> baseResult = await Get(@base.Name);
        if (!baseResult.Success)
        {
            Log.Error("Failed to get user {user} from Helix", @base.Name);
            return new Result<ExtendedChannel>(default!, false, new ArgumentNullException());
        }

        User value = baseResult.Value;


        return new Result<ExtendedChannel>(new(value.DisplayName, value.Username, value.Id, value.AvatarUrl, value.CreatedAt, @base.Priority, @base.Logged), true, default!);
    }

    private User(HelixUser helixUser)
    {
        DisplayName = helixUser.DisplayName;
        Username = helixUser.Login;
        Id = helixUser.Id;
        Type = helixUser.Type;
        BroadcasterType = helixUser.BroadcasterType;
        CreatedAt = helixUser.CreatedAt;
        Description = helixUser.Description;
        Email = helixUser.Email;
        AvatarUrl = helixUser.ProfileImageUrl;
        OfflineImageUrl = helixUser.OfflineImageUrl;
    }
}

public sealed record PartialUser(string Displayname, string Username, string ID);
public sealed record ExtendedChannel(string Displayname, string Username, string ID, string AvatarUrl, DateTime DateJoined, int Priority, bool Logged);