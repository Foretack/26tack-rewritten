using Tack.Handlers;
using Tack.Nonclass;
using Tack.Json;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal class SteelPath : Command
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

        SteelPathRewards? rewards = ObjectCache.Get<SteelPathRewards>("steelpath_wf");
        if (rewards is null)
        {
            var r = await ExternalAPIHandler.WarframeStatusApi<SteelPathRewards>("steelPath");
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, There was an error getting Steel Path data :( ({r.Exception.Message})");
                return;
            }
            rewards = r.Value;
        }

        TimeSpan timeLeft = rewards.expiry - DateTime.Now;
        if (timeLeft.TotalSeconds <= 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Data is outdated. Try again later?");
            return;
        }

        string rewardsString = $"Current item in rotation: {rewards.currentReward.name} ({rewards.currentReward.cost} Steel Essence) 🏹 ●";
        string nextInRotationString = $" Next in rotation: {rewards.rotation[0].name}";
        MessageHandler.SendMessage(channel, $"@{user}, {rewardsString} {nextInRotationString} ➜ in: {timeLeft.FormatTimeLeft()}");
        ObjectCache.Put("steelpath_wf", rewards, (int)timeLeft.TotalSeconds);
    }
}
