using System.Net;
using System.Text.Json;
using Tack.Handlers;
using Tack.Json;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal class Profile : Command
{
    public override CommandInfo Info { get; } = new(
        name: "profile",
        description: "Get information about a player's profile data",
        aliases: new string[] { "user", "account", "acc" },
        userCooldown: 10,
        channelCooldown: 3
    );

    private static readonly HttpClient _requests = new HttpClient();
    private const string WF_ARSENAL_ID = "ud1zj704c0eb1s553jbkayvqxjft97";
    private const int LeechedChannel = 95178769;

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        string? token = ObjectCache.Get<string>("v5_ext_token") ?? await GetV5Token(LeechedChannel);
        if (token is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, An error occured with token generation. Please report this issue :( ");
            return;
        }
        ObjectCache.Put("v5_ext_token", token, 60);

        string target = ctx.Args.Length == 0 ? ctx.IrcMessage.Username : ctx.Args[0];
        var data = await GetProfileDataStream(target, token);
        if (data.Code == HttpStatusCode.NoContent && ctx.IrcMessage.Username == target.ToLower())
        {
            MessageHandler.SendMessage(channel, $"@{user}, You have loadout sharing disabled. Enable it here under data permissions: https://www.warframe.com/user");
            return;
        }
        if (data.Code == HttpStatusCode.NoContent)
        {
            MessageHandler.SendMessage(channel, $"@{user}, User does not have loadout sharing enabled. :/");
            return;
        }
        if (data.Code != HttpStatusCode.OK)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Account Data Not Found. The requested account doesn't exist or isn't public. :/");
            return;
        }

        ProfileRoot? profile = await JsonSerializer.DeserializeAsync<ProfileRoot>(data.Stream!);
        if (profile is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, There was an error parsing your data :( ");
            return;
        }

        string name = profile.AccountInfo.PlayerName;
        int mr = profile.AccountInfo.MasteryRank;
        string lastUpdated = (DateTime.Now - DateTimeOffset.FromUnixTimeSeconds(1660165795)).FormatTimeLeft();
        MessageHandler.SendMessage(channel, $"@{user}, \"{name}\" MasteryRank: {mr} (last updated {lastUpdated} ago)");
    }


    private async Task<string?> GetV5Token(int channelId)
    {
        // this is not mine
        if (!_requests.DefaultRequestHeaders.Contains("client-id")) _requests.DefaultRequestHeaders.Add("client-id", "kimne78kx3ncx6brgo4mv6wki5h1ko");
        try
        {
            Stream data = await _requests.GetStreamAsync($"https://api.twitch.tv/v5/channels/{channelId}/extensions");
            V5Root? v5r = await JsonSerializer.DeserializeAsync<V5Root>(data);
            if (v5r is null) return null;
            return (
                   from ext in v5r.Tokens
                   where ext.ExtensionId == WF_ARSENAL_ID
                   select ext.Key
                   ).First();
        }
        catch { return null; }
    }

    private async Task<(HttpStatusCode Code, Stream? Stream)> GetProfileDataStream(string username, string token)
    {
        HttpClient c = new HttpClient();
        c.DefaultRequestHeaders.Add("Origin", $"https://{WF_ARSENAL_ID}.ext-twitch.tv");
        c.DefaultRequestHeaders.Add("Referer", $"https://{WF_ARSENAL_ID}.ext-twitch.tv/");
        c.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var data = await c.GetAsync($"https://content.warframe.com/dynamic/twitch/getActiveLoadout.php?account={username.ToLower()}");

        c.Dispose();
        return (data.StatusCode, data.Content.ReadAsStream());
    }
}
