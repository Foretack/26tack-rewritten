using System.Text.Json;
using Serilog;
using Tack.Json;
using Tack.Models;
using Tack.Database;

namespace Tack.Handlers;
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
        reqs.Timeout = TimeSpan.FromMilliseconds(1500);

        try
        {
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

    #region Warframe
    private const string WarframeBaseUrl = "https://api.warframestat.us/pc";

    public static async Task<CetusCycle?> GetCetusCycle()
    {
        HttpClient requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(1);

        try
        {
            Stream cResponse = await requests.GetStreamAsync(WarframeBaseUrl + "/cetusCycle");
            CetusCycle cycle = (await JsonSerializer.DeserializeAsync<CetusCycle>(cResponse))!;
            return cycle;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to fetch cetus cycle fdm");
            DbQueries db = new DbQueries();
            await db.LogException(ex);
            return null;
        }
    }

    public static async Task<MarketItems?> GetMarketItemListings(string itemName)
    {
        HttpClient requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(1);

        try
        {
            Stream iResponse = await requests.GetStreamAsync($"https://api.warframe.market/v1/items/{itemName}/orders?platform=pc");
            MarketItems item = (await JsonSerializer.DeserializeAsync<MarketItems>(iResponse))!;
            return item;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to fetch \"{itemName}\" from the market");
            return null;
        }
    }

    public static async Task<RelicData?> GetRelicData()
    {
        HttpClient requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(5);

        try
        {
            Stream rResponse = await requests.GetStreamAsync("http://drops.warframestat.us/data/relics.json");
            RelicData relics = (await JsonSerializer.DeserializeAsync<RelicData>(rResponse))!;
            return relics;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to fetch relic data");
            DbQueries db = new DbQueries();
            await db.LogException(ex);
            return null;
        }
    }

    public static async Task<Result<T>> WarframeStatusApi<T>(string endpoint, string platform = "pc", string language = "en", int timeout = 5) 
    {
        HttpClient caller = new HttpClient();
        caller.Timeout = TimeSpan.FromSeconds(timeout);

        try
        {
            string pStr = string.IsNullOrEmpty(platform) ? string.Empty : $"{platform}/";
            string langStr = string.IsNullOrEmpty(language) ? string.Empty : $"?lang={language}";
            Stream response = await caller.GetStreamAsync($"{WarframeBaseUrl}/{pStr}{endpoint}{langStr}");
            caller.Dispose();
            T value =  (await JsonSerializer.DeserializeAsync<T>(response))!;
            return new Result<T>(value, true, default!);
        }
        catch (TaskCanceledException tex)
        {
            Log.Warning($"Call for `{typeof(T)}` timed out ({timeout}s)");
            return new Result<T>(default!, false, tex);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Exception thrown during api call for `{typeof(T)}`");
            return new Result<T>(default!, false, ex);
        }
    }
    #endregion
}

public record struct Result<T>(T Value, bool Success, Exception Exception);
