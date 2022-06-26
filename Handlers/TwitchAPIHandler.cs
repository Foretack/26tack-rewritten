using Serilog;
using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace Tack.Handlers;
internal static class TwitchAPIHandler
{
    #region Properties
    public static readonly TwitchAPI API = new TwitchAPI(settings: new ApiSettings { AccessToken = Config.Auth.AccessToken, ClientId = Config.Auth.ClientID });

    private static readonly Helix Helix = API.Helix;
    #endregion

    #region Users
    internal static async Task<User?> GetUsers(string username)
    {
        User[]? u = await GetUsers(new List<string> { username });
        return u?[0];
    }
    internal static async Task<User[]?> GetUsers(string[] usernames)
    {
        User[]? u = await GetUsers(usernames.ToList());
        return u;
    }
    internal static async Task<User?> GetUsersByID(string id)
    {
        User[]? u = await GetUsers(ids: new List<string> { id });
        return u?[0];
    }
    internal static async Task<User[]?> GetUsersByID(string[] ids)
    {
        User[]? u = await GetUsers(ids: ids.ToList());
        return u;
    }

    private static async Task<User[]?> GetUsers(List<string>? logins = null, List<string>? ids = null)
    {
        try
        {
            GetUsersResponse us = logins is not null ? await Helix.Users.GetUsersAsync(logins: logins) : await Helix.Users.GetUsersAsync(ids: ids);
            return us.Users;
        }
        catch (TooManyRequestsException _a)
        {
            Log.Error(_a, $"Failed to fetch users: {string.Join(';', logins ?? ids!)}");
            return null;
        }
        catch (Exception _c)
        {
            Log.Error(_c, $"how did I get here");
            return null;
        }
    }
    #endregion
}
