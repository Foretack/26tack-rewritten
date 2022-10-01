using Tack.Database;
using Tack.Handlers;
using Tack.Json;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal sealed class SteelPath : Command
{
    public override CommandInfo Info { get; } = new(
        name: "steelpath",
        description: "Get the current Steel Path rotation & the next one",
        aliases: new string[] { "sp", "path" },
        userCooldown: 5,
        channelCooldown: 3
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        SteelPathRewards rewards = await "warframe:steelpath:rewards".GetOrCreate<SteelPathRewards>(async () =>
        {
            var r = await ExternalAPIHandler.WarframeStatusApi<SteelPathRewards>("steelPath");
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, ⚠ Http error! {r.Exception.Message}");
                return default!;
            }
            return r.Value;
        }, true);
        if (rewards is null) return;

        if (Time.HasPassed(rewards.Expiry))
        {
            await "warframe:steelpath:rewards".RemoveKey();
            MessageHandler.SendMessage(channel, $"@{user}, Data is outdated. Try again later?");
            return;
        }
        await "warframe:steelpath:rewards".SetKeyExpiry(Time.Until(rewards.Expiry));

        string rewardsString = $"Current item in rotation: {rewards.CurrentReward.Name} ({rewards.CurrentReward.Cost} Steel Essence) 🏹 ●";
        string nextInRotationString = $" Next in rotation: {rewards.Rotation[0].Name}";
        MessageHandler.SendMessage(channel, $"@{user}, {rewardsString} {nextInRotationString} ➜ in: {Time.UntilString(rewards.Expiry)}");
    }
}
