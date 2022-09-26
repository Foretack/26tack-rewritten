using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix;
using TLU = TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace Tack.Handlers;
internal static class TwitchAPIHandler
{
    #region Properties
    public static readonly TwitchAPI API = new(settings: new ApiSettings { AccessToken = Config.Auth.AccessToken, ClientId = Config.Auth.ClientID });

    private static readonly Helix Helix = API.Helix;
    #endregion

    #region Users
    internal static async Task<TLU::User?> GetUsers(string username)
    {
        TLU::User[]? u = await GetUsers(new List<string> { username });
        return u.Length > 0 ? u[0] : null;
    }
    internal static async Task<TLU::User[]?> GetUsers(string[] usernames)
    {
        TLU::User[]? u = await GetUsers(usernames.ToList());
        return u.Length > 0 ? u : null;
    }
    internal static async Task<TLU::User?> GetUsersByID(string id)
    {
        TLU::User[]? u = await GetUsers(ids: new List<string> { id });
        return u.Length > 0 ? u[0] : null;
    }
    internal static async Task<TLU::User[]?> GetUsersByID(string[] ids)
    {
        TLU::User[]? u = await GetUsers(ids: ids.ToList());
        return u.Length > 0 ? u : null;
    }

    private static async Task<TLU::User[]> GetUsers(List<string>? logins = null, List<string>? ids = null)
    {
        try
        {
            TLU::GetUsersResponse us = logins is not null ? await Helix.Users.GetUsersAsync(logins: logins) : await Helix.Users.GetUsersAsync(ids: ids);
            return us.Users;
        }
        catch (TooManyRequestsException _a)
        {
            Log.Error(_a, $"Failed to fetch users: {string.Join(';', logins ?? ids!)}");
            return Array.Empty<TLU::User>();
        }
        catch (Exception _c)
        {
            Log.Error(_c, $"how did I get here");
            return Array.Empty<TLU::User>();
        }
    }
    #endregion
}
