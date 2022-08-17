using System.Net;
using System.Text.Json;
using Serilog;
using Tack.Database;
using Tack.Json;
using M = Tack.Models;

namespace Tack.Handlers;
internal static class ExternalAPIHandler
{
    public static async Task<M::User?> GetIvrUser(string username)
    {
        var requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(2);

        try
        {
            Stream resp = await requests.GetStreamAsync($"https://api.ivr.fi/twitch/resolve/{username}");
            IvrUserData ivrUser = (await JsonSerializer.DeserializeAsync<IvrUserData>(resp))!;
            var user = new M::User(ivrUser.DisplayName, ivrUser.Login, ivrUser.Id.ToString(), ivrUser.Logo, ivrUser.CreatedAt);
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
        var requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(2);

        try
        {
            Stream tmiResponse = await requests.GetStreamAsync($"https://tmi.twitch.tv/group/user/{channel}/chatters");
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
        var requests = new HttpClient();
        requests.Timeout = TimeSpan.FromMilliseconds(500);

        try
        {
            Stream response = await requests.GetStreamAsync("https://v2.jokeapi.dev/joke/Any?blacklistFlags=religious,racist&type=single");
            JokeAPI rj = (await JsonSerializer.DeserializeAsync<JokeAPI>(response))!;
            return rj;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get a random joke xd");
            return null;
        }
    }

    public static async Task<JustLog> GetIvrChannels()
    {
        var requests = new HttpClient();
        requests.Timeout = TimeSpan.FromMilliseconds(1500);

        try
        {
            Stream jlcl = await requests.GetStreamAsync(Config.Links.IvrChannels);
            JustLog deserialized = (await JsonSerializer.DeserializeAsync<JustLog>(jlcl))!;
            return deserialized;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to load just logged channels");
            throw;
        }
    }

    #region Warframe
    private const string WarframeBaseUrl = "https://api.warframestat.us/";

    public static async Task<CetusCycle?> GetCetusCycle()
    {
        var requests = new HttpClient();
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
            var db = new DbQueries();
            _ = await db.LogException(ex);
            return null;
        }
    }

    public static async Task<MarketItems?> GetMarketItemListings(string itemName)
    {
        var requests = new HttpClient();
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
        var requests = new HttpClient();
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
            var db = new DbQueries();
            _ = await db.LogException(ex);
            return null;
        }
    }

    public static async Task<Result<T>> WarframeStatusApi<T>(string endpoint, string platform = "pc", string language = "en", int timeout = 5)
    {
        var requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(timeout);

        try
        {
            string pStr = string.IsNullOrEmpty(platform) ? string.Empty : $"{platform}/";
            string langStr = string.IsNullOrEmpty(language) ? string.Empty : $"?lang={language}";
            string url = $"{WarframeBaseUrl}{pStr}{endpoint}{langStr}";
            Stream response = await requests.GetStreamAsync(url);
            Log.Debug($"called {url} [{typeof(T)}]");
            requests.Dispose();
            T value = (await JsonSerializer.DeserializeAsync<T>(response))!;
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

    public static async Task<Result<string>> FindFromUniqueName(string category, string uniqueName, int timeout = 10)
    {
        var requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(timeout);

        try
        {
            Stream data = await requests.GetStreamAsync($"https://raw.githubusercontent.com/WFCD/warframe-items/master/data/json/{category}.json");
            WarframeItem[]? items = await JsonSerializer.DeserializeAsync<WarframeItem[]>(data);
            string name = items!.First(x => x.UniqueName == uniqueName).NormalName;
            return new Result<string>(name, true, default!);
        }
        catch (TaskCanceledException tex) { return new Result<string>(default!, false, tex); }
        catch (Exception ex) { return new Result<string>(default!, false, ex); }
    }
    #endregion

    #region Warframe arsenal
    private const string WF_ARSENAL_ID = "3yumzvi6r4wfycsk7vt1kbtto9s0n3";
    public static async Task<Result<string>> GetWarframeTwitchExtensionTokenV5(int fromChannel)
    {
        var requests = new HttpClient();
        requests.DefaultRequestHeaders.Add("client-id", "kimne78kx3ncx6brgo4mv6wki5h1ko");
        requests.Timeout = TimeSpan.FromSeconds(5);

        try
        {
            Stream data = await requests.GetStreamAsync($"https://api.twitch.tv/v5/channels/{fromChannel}/extensions");
            V5Root? v5r = await JsonSerializer.DeserializeAsync<V5Root>(data);
            return new Result<string>(
                v5r!.Tokens.First(x => x.ExtensionId == WF_ARSENAL_ID).Key
                , true, default!);
        }
        catch (Exception ex) { return new Result<string>(default!, false, ex); }
    }

    public static async Task<Result<(Stream? Stream, HttpStatusCode Code)>> GetWarframeProfileData(string username, string extensionKey)
    {
        var requests = new HttpClient();
        requests.DefaultRequestHeaders.Add("Origin", $"https://{WF_ARSENAL_ID}.ext-twitch.tv");
        requests.DefaultRequestHeaders.Add("Referer", $"https://{WF_ARSENAL_ID}.ext-twitch.tv/");
        requests.DefaultRequestHeaders.Add("Authorization", $"Bearer {extensionKey}");
        requests.Timeout = TimeSpan.FromSeconds(10);

        try
        {
            HttpResponseMessage data = await requests.GetAsync($"https://content.warframe.com/dynamic/twitch/getActiveLoadout.php?account={username.ToLower()}");
            return new Result<(Stream? Stream, HttpStatusCode Code)>((await data.Content.ReadAsStreamAsync(), data.StatusCode), true, default!);
        }
        catch (Exception ex) { return new Result<(Stream? Stream, HttpStatusCode Code)>(default!, false, ex); }
    }
    #endregion
}

public record struct Result<T>(T Value, bool Success, Exception Exception);
