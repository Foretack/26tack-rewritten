using System.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tack.Database;
using Tack.Models;

namespace Tack.Handlers;
internal static class ExternalApiHandler
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public static async Task<Result<T>> GetInto<T>(string url, int timeout = 5)
    {
        var requests = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(timeout)
        };

        try
        {
            HttpResponseMessage response = await requests.GetAsync(url);
            T? result = await response.Content.ReadFromJsonAsync<T>();
            if (result is null)
            {
                return new Result<T>(result!, false, new JsonException("Failed to serialize"));
            }

            return new Result<T>(result!, true, default!);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "GET {url} => {message}", url, ex.Message);
            return new Result<T>(default!, false, ex);
        }
    }

    public static async Task<IvrUser> GetIvrUser(string username)
    {
        var requests = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2)
        };

        IvrUser[]? users = await requests.GetFromJsonAsync<IvrUser[]>($"https://api.ivr.fi/v2/twitch/user?login={username}")
            ?? throw new NoNullAllowedException("Users is null");

        return users.First();
    }

    public static async Task<IvrUser[]> GetIvrUsersById(int[] ids)
    {
        var requests = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(ids.Length * 2)
        };
        requests.DefaultRequestHeaders.Add("User-Agent", "occluder");

        string url = $"https://api.ivr.fi/v2/twitch/user?id={string.Join(',', ids)}";
        HttpResponseMessage get = await requests.GetAsync(url);

        Log.Debug("GET {StatusCode} {url}", get.StatusCode, url);

        IvrUser[]? users = await get.Content.ReadFromJsonAsync<IvrUser[]>(_jsonOptions)
            ?? throw new NoNullAllowedException("Users is null");

        return users;
    }

    #region Warframe
    private const string WarframeBaseUrl = "https://api.warframestat.us/";

    public static async Task<RelicData?> GetRelicData()
    {
        var requests = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        try
        {
            Stream rResponse = await requests.GetStreamAsync("http://drops.warframestat.us/data/relics.json");
            RelicData relics = (await JsonSerializer.DeserializeAsync<RelicData>(rResponse))!;
            return relics;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to fetch relic data");
            DbQueries db = new SingleOf<DbQueries>();
            _ = await db.LogException(ex);
            return null;
        }
    }

    public static async Task<Result<T>> WarframeStatusApi<T>(string endpoint, string platform = "pc", string language = "en", int timeout = 5)
    {
        var requests = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeout)
        };

        try
        {
            string pStr = string.IsNullOrEmpty(platform) ? string.Empty : $"{platform}/";
            string langStr = string.IsNullOrEmpty(language) ? string.Empty : $"?lang={language}";
            string url = $"{WarframeBaseUrl}{pStr}{endpoint}{langStr}";
            Stream response = await requests.GetStreamAsync(url);
            requests.Dispose();
            T value = (await JsonSerializer.DeserializeAsync<T>(response))!;
            Log.Verbose("called {url} [{value}]", url, value);
            return new Result<T>(value, true, default!);
        }
        catch (TaskCanceledException tex)
        {
            Log.Warning("Call for `{type}` timed out ({timeout}s)", typeof(T), timeout);
            return new Result<T>(default!, false, tex);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception thrown during api call for `{type}`", typeof(T));
            return new Result<T>(default!, false, ex);
        }
    }

    public static async Task<Result<string>> FindFromUniqueName(string category, string uniqueName, int timeout = 10)
    {
        var requests = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeout)
        };

        try
        {
            Stream data = await requests.GetStreamAsync($"https://raw.githubusercontent.com/WFCD/warframe-items/master/data/json/{category}.json");
            WarframeItem[]? items = await JsonSerializer.DeserializeAsync<WarframeItem[]>(data);
            string name = items!.First(x => x.UniqueName == uniqueName).NormalName;
            return new Result<string>(name, true, default!);
        }
        catch (TaskCanceledException tex)
        {
            return new Result<string>(default!, false, tex);
        }
        catch (Exception ex)
        {
            return new Result<string>(default!, false, ex);
        }
    }
    #endregion

    #region Warframe arsenal
    private const string WF_ARSENAL_ID = "ud1zj704c0eb1s553jbkayvqxjft97";
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
        catch (Exception ex)
        {
            return new Result<string>(default!, false, ex);
        }
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
        catch (Exception ex)
        {
            return new Result<(Stream? Stream, HttpStatusCode Code)>(default!, false, ex);
        }
    }
    #endregion
}

public record struct Result<T>(T Value, bool Success, Exception Exception);
