using System.Text.Json;
using _26tack_rewritten.json;
using _26tack_rewritten.models;
using Serilog;

namespace _26tack_rewritten.handlers;
internal static class ExternalAPIHandler
{
    public static async Task<User?> GetIvrUser(string username)
    {
        HttpClient reqs = new HttpClient();
        reqs.Timeout = TimeSpan.FromSeconds(2);

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

    public static async Task<TMI?> GetChannelChatters(string channel)
    {
        HttpClient reqs = new HttpClient();
        reqs.Timeout = TimeSpan.FromSeconds(2);

        try
        {
            Stream tmiResponse = await reqs.GetStreamAsync($"https://tmi.twitch.tv/group/user/{channel}/chatters");
            TMI clist = (await JsonSerializer.DeserializeAsync<TMI>(tmiResponse))!;
            return clist;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to fetch [{channel}] chatters");
            return null;
        }
    }

    public static async Task<JokeAPI?> GetRandomJoke()
    {
        HttpClient reqs = new HttpClient();
        reqs.Timeout = TimeSpan.FromMilliseconds(500);

        try
        {
            Stream response = await reqs.GetStreamAsync("https://v2.jokeapi.dev/joke/Any?blacklistFlags=religious,racist&type=single");
            JokeAPI rj = (await JsonSerializer.DeserializeAsync<JokeAPI>(response))!;
            return rj;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get a random joke xd");
            return null;
        }
    }

    public static async Task<JustLogLoggedChannels> GetIvrChannels()
    {
        HttpClient reqs = new HttpClient();

        try
        {
            reqs.Timeout = TimeSpan.FromMilliseconds(1500);
            Stream jlcl = await reqs.GetStreamAsync(Config.Links.IvrChannels);
            JustLogLoggedChannels deserialized = (await JsonSerializer.DeserializeAsync<JustLogLoggedChannels>(jlcl))!;
            return deserialized;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to load just logged channels");
            throw;
        }
    }
}
