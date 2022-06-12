using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.models;
using _26tack_rewritten.json;
using _26tack_rewritten.utils;

namespace _26tack_rewritten.commands.warframeset;
internal class SteelPath : DataCacher<SteelPathRewards>, IChatCommand
{
    public Command Info()
    {
        string name = "steelpath";
        string description = "Get the current Steel Path rotation & the next one";
        string[] aliases = { "sp", "path" };
        int[] cooldowns = { 10, 3 };

        return new Command(name, description, aliases, cooldowns);
    }

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        SteelPathRewards? rewards = GetCachedPiece("steelpath")?.Object 
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

        string timeLeftString = timeLeft.TotalDays >= 1 ? $"{timeLeft:d'd'h'h'}" : $"{timeLeft:h'h'm'm's's'}";
        string rewardsString = $"Current item in rotation: {rewards.currentReward.name} ({rewards.currentReward.cost} Steel Essence) --";
        string nextInRotationString = $"🏹 Next in rotation: {rewards.rotation[0].name}";
        MessageHandler.SendMessage(channel, $"@{user}, {rewardsString} {nextInRotationString} ➡ in: {timeLeftString}");
        CachePiece("steelpath", rewards, (int)timeLeft.TotalSeconds);
    }
}
