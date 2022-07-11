using Tack.Handlers;
using Tack.Interfaces;
using Tack.Json;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal class SteelPath : IChatCommand
{
    public Command Info() => new(
        name: "steelpath",
        description: "Get the current Steel Path rotation & the next one",
        aliases: new string[] { "sp", "path" },
        cooldowns: new int[] { 5, 3 }
        );

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        SteelPathRewards? rewards = ObjectCache.Get<SteelPathRewards>("steelpath_wf")
            ?? await ExternalAPIHandler.GetSteelPathRewards();
        if (rewards is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, There was an error getting Steel Path data :(");
            return;
        }

        TimeSpan timeLeft = rewards.expiry - DateTime.Now;
        if (timeLeft.TotalSeconds <= 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Data is outdated. Try again later?");
            return;
        }

        string rewardsString = $"Current item in rotation: {rewards.currentReward.name} ({rewards.currentReward.cost} Steel Essence) 🏹 ●";
        string nextInRotationString = $" Next in rotation: {rewards.rotation[0].name}";
        MessageHandler.SendMessage(channel, $"@{user}, {rewardsString} {nextInRotationString} ➡ in: {timeLeft.FormatTimeLeft()}");
        ObjectCache.Put("steelpath_wf", rewards, (int)timeLeft.TotalSeconds);
    }
}
