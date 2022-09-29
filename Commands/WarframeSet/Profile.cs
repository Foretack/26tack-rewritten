using System.Net;
using System.Text;
using System.Text.Json;
using Tack.Database;
using Tack.Handlers;
using Tack.Json;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal sealed class Profile : Command
{
    public override CommandInfo Info { get; } = new(
        name: "profile",
        description: "Get information about a player's profile data",
        aliases: new string[] { "user", "account", "acc" },
        userCooldown: 10,
        channelCooldown: 3
    );

    private const int LeechedChannel = 35833485;
    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        string token = await "warframe:v5_ext_token".GetOrCreate(async () =>
        {
            var t = await ExternalAPIHandler.GetWarframeTwitchExtensionTokenV5(LeechedChannel);
            if (!t.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, An error occured with token generation. Please report this issue :( ");
                Log.Warning(t.Exception, $"Error generating Warframe extension token");
                return default!;
            }
            return t.Value;
        }, true, TimeSpan.FromMinutes(1));
        if (string.IsNullOrEmpty(token)) return;

        string target = ctx.Args.Length == 0 ? ctx.IrcMessage.Username : ctx.Args[0];
        Result<(Stream? Stream, HttpStatusCode Code)> data = await ExternalAPIHandler.GetWarframeProfileData(target, token);
        if (data.Value.Code == HttpStatusCode.NoContent)
        {
            MessageHandler.SendMessage(channel, $"@{user}, User does not have loadout sharing enabled. Loudout sharing can be enabled under data permissions on: https://www.warframe.com/user");
            return;
        }
        if (data.Value.Code != HttpStatusCode.OK)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Account Data Not Found. The requested account doesn't exist or isn't public. :/");
            return;
        }

        ProfileRoot? profile = await JsonSerializer.DeserializeAsync<ProfileRoot>(data.Value.Stream!);
        if (profile is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, There was an error parsing your data. Saj ");
            return;
        }

        string name = profile.AccountInfo.PlayerName;
        int mr = profile.AccountInfo.MasteryRank;
        TimeSpan lastUpdated = DateTime.Now - DateTimeOffset.FromUnixTimeSeconds(profile.AccountInfo.LastUpdated).LocalDateTime;
        string warframe = (await ExternalAPIHandler.FindFromUniqueName("Warframes", profile.LoadOuts.NORMAL.Warframe.UniqueName)).Value ?? "{unknown}";
        string primary = (await ExternalAPIHandler.FindFromUniqueName("Primary", profile.LoadOuts.NORMAL.Primary.UniqueName)).Value ?? "{unknown}";
        string secondary = (await ExternalAPIHandler.FindFromUniqueName("Secondary", profile.LoadOuts.NORMAL.Secondary.UniqueName)).Value ?? "{unknown}";
        string melee = (await ExternalAPIHandler.FindFromUniqueName("Melee", profile.LoadOuts.NORMAL.Melee.UniqueName)).Value ?? "{unknown}";

        var message = new StringBuilder($"@{user}, ");
        _ = message
            .Append($"\"{name}\" ")
            .Append($"MasteryRank: {mr}, ")
            .Append("Equipped: [")
            .Append($"{warframe}, {primary}, {secondary}, {melee}")
            .Append($"] ")
            .Append(lastUpdated.TotalSeconds > 59 ? $"(last updated {lastUpdated.FormatTimeLeft()} ago)" : string.Empty);

        MessageHandler.SendMessage(channel, message.ToString());
    }
}
