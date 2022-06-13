using System.Text.Json;
using _26tack_rewritten.json;
using _26tack_rewritten.models;
using Serilog;
using _26tack_rewritten.database;

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

    private const string WarframeBaseUrl = "https://api.warframestat.us/pc";
    public static async Task<Fissure[]?> GetFissures()
    {
        HttpClient requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(1);

        try
        {
            Stream fResponse = await requests.GetStreamAsync(WarframeBaseUrl + "/fissures");
            Fissure[] fissures = (await JsonSerializer.DeserializeAsync<Fissure[]>(fResponse))!;
            return fissures;
        }
         catch (Exception ex)
        {
            Log.Error(ex, $"Failed to fetch current fissures fdm");
            Database db = new Database();
            await db.LogException(ex);
            return null;
        }
    }

    public static async Task<Alert[]?> GetAlerts()
    {
        HttpClient requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(1);

        try
        {
            Stream aResponse = await requests.GetStreamAsync(WarframeBaseUrl + "/alerts");
            Alert[] alerts = (await JsonSerializer.DeserializeAsync<Alert[]>(aResponse))!;
            return alerts;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to fetch current alerts fdm");
            Database db = new Database();
            await db.LogException(ex);
            return null;
        }
    }

    public static async Task<CurrentSortie?> GetSortie()
    {
        HttpClient requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(1);

        try
        {
            Stream aResponse = await requests.GetStreamAsync(WarframeBaseUrl + "/sortie?language=en");
            CurrentSortie sortie = (await JsonSerializer.DeserializeAsync<CurrentSortie>(aResponse))!;
            return sortie;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to fetch current sortie fdm");
            Database db = new Database();
            await db.LogException(ex);
            return null;
        }
    }

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
            Database db = new Database();
            await db.LogException(ex);
            return null;
        }
    }

    public static async Task<VallisCycle?> GetVallisCycle()
    {
        HttpClient requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(1);

        try
        {
            Stream cResponse = await requests.GetStreamAsync(WarframeBaseUrl + "/vallisCycle");
            VallisCycle cycle = (await JsonSerializer.DeserializeAsync<VallisCycle>(cResponse))!;
            return cycle;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to fetch vallis cycle fdm");
            Database db = new Database();
            await db.LogException(ex);
            return null;
        }
    }

    public static async Task<CambionCycle?> GetCambionCycle()
    {
        HttpClient requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(1);

        try
        {
            Stream cResponse = await requests.GetStreamAsync(WarframeBaseUrl + "/cambionCycle");
            CambionCycle cycle = (await JsonSerializer.DeserializeAsync<CambionCycle>(cResponse))!;
            return cycle;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to fetch cambion cycle fdm");
            Database db = new Database();
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
            Database db = new Database();
            await db.LogException(ex);
            return null;
        }
    }

    public static async Task<InvasionNode[]?> GetInvasions()
    {
        HttpClient requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(2);

        try
        {
            Stream iResponse = await requests.GetStreamAsync("https://api.warframestat.us/pc/invasions?lang=en");
            InvasionNode[] invasions = (await JsonSerializer.DeserializeAsync<InvasionNode[]>(iResponse))!;
            return invasions;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to fetch invasions");
            Database db = new Database();
            await db.LogException(ex);
            return null;
        }
    }

    public static async Task<SteelPathRewards?> GetSteelPathRewards()
    {
        HttpClient requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(1);

        try
        {
            Stream rResponse = await requests.GetStreamAsync(WarframeBaseUrl + "/steelPath");
            SteelPathRewards rewards = (await JsonSerializer.DeserializeAsync<SteelPathRewards>(rResponse))!;
            return rewards;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to fetch steel path rewards");
            Database db = new Database();
            await db.LogException(ex);
            return null;
        }
    }

    public static async Task<ItemDropData[]?> GetItemDropData(string itemName)
    {
        HttpClient requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(2.5);

        try
        {
            Stream iResponse = await requests.GetStreamAsync($"https://api.warframestat.us/drops/search/{itemName}");
            ItemDropData[] rewards = (await JsonSerializer.DeserializeAsync<ItemDropData[]>(iResponse))!;
            return rewards;
        }
        catch (Exception ex)
        {
            if (ex is TaskCanceledException)
            {
                Log.Error( $"Fetching drop data for \"{itemName}\" timed out");
            }
            else
            {
                Log.Error(ex, $"Failed to fetch drop data for \"{itemName}\"");
            }
            return null;
        }
    }

    public static async Task<ModInfo?> GetModInfo(string modName)
    {
        HttpClient requests = new HttpClient();
        requests.Timeout = TimeSpan.FromSeconds(2.5);

        try
        {
            Stream mResponse = await requests.GetStreamAsync($"https://api.warframestat.us/mods/{modName}");
            ModInfo mod = (await JsonSerializer.DeserializeAsync<ModInfo>(mResponse))!;
            return mod;
        }
        catch (Exception ex)
        {
            if (ex is TaskCanceledException)
            {
                Log.Error($"Fetching info about \"{modName}\" timed out");
            }
            else
            {
                Log.Error(ex, $"Failed to fetch info about \"{modName}\"");
            }
            return null;
        }
    }
}
