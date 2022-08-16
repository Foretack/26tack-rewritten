using System.Net;
using System.Text;
using System.Text.Json;
using Serilog;
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

    private const int LeechedChannel = 35833485;
    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        string? token = ObjectCache.Get<string>("v5_ext_token") 
            ?? (await ExternalAPIHandler.GetWarframeTwitchExtensionTokenV5(LeechedChannel)).Value;
        if (token is null)
        {
            var t = await ExternalAPIHandler.GetWarframeTwitchExtensionTokenV5(LeechedChannel);
            if (!t.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, An error occured with token generation. Please report this issue :( ");
                Log.Error(t.Exception, $"Error generating Warframe extension token");
                return;
            }
            token = t.Value;
        }
        ObjectCache.Put("v5_ext_token", token, 60);

        string target = ctx.Args.Length == 0 ? ctx.IrcMessage.Username : ctx.Args[0];
        var data = await ExternalAPIHandler.GetWarframeProfileData(target, token);
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
        TimeSpan lastUpdated = (DateTime.Now - DateTimeOffset.FromUnixTimeSeconds(profile.AccountInfo.LastUpdated).LocalDateTime);
        string warframe = (await ExternalAPIHandler.FindFromUniqueName("Warframes", profile.LoadOuts.NORMAL.Warframe.UniqueName)).Value ?? "{unknown}";
        string primary = (await ExternalAPIHandler.FindFromUniqueName("Primary", profile.LoadOuts.NORMAL.Primary.UniqueName)).Value ?? "{unknown}";
        string secondary = (await ExternalAPIHandler.FindFromUniqueName("Secondary", profile.LoadOuts.NORMAL.Secondary.UniqueName)).Value ?? "{unknown}";
        string melee = (await ExternalAPIHandler.FindFromUniqueName("Melee", profile.LoadOuts.NORMAL.Melee.UniqueName)).Value ?? "{unknown}";

        var message = new StringBuilder($"@{user}, ");
        message
            .Append($"\"{name}\" ")
            .Append($"MasteryRank: {mr}, ")
            .Append("Equipped: [")
            .Append($"{warframe}, {primary}, {secondary}, {melee}")
            .Append($"] ")
            .Append(lastUpdated.TotalSeconds > 59 ? $"(last updated {lastUpdated} ago)" : string.Empty);

        MessageHandler.SendMessage(channel,  message.ToString());
    }
}
