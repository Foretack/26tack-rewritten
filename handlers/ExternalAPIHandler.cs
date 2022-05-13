using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using _26tack_rewritten.json;
using _26tack_rewritten.models;
using Serilog;

namespace _26tack_rewritten.handlers;
internal static class ExternalAPIHandler
{
    public static async Task<User?> GetIvrUser(string username)
    {
        HttpClient reqs = new HttpClient();

        try
        {
            Stream resp = await reqs.GetStreamAsync($"https://api.ivr.fi/twitch/resolve/{username}");
            IvrUserData ivrUser = (await JsonSerializer.DeserializeAsync<IvrUserData>(resp))!;
            User user = new User(ivrUser.displayName, ivrUser.login, ivrUser.id, ivrUser.logo, ivrUser.createdAt);
            return user;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to resolve user data from Ivr");
            return null;
        }
    }
}
