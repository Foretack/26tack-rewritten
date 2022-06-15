using Tack.Handlers;
using Tack.Interfaces;
using Tack.Json;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal class Mods : IChatCommand
{
    public Command Info()
    {
        string name = "modinfo";
        string description = "Get the stats of the closest matching mod. Additional options: `rank:number` (highest rank by default)";
        string[] aliases = { "mod", "mods" };
        int[] cooldowns = { 5, 3 };

        return new Command(name, description, aliases, cooldowns);
    }

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Specify the mod you're looking for FeelsDankMan");
            return;
        }

        string modName = string.Join(' ', args.Where(x => !x.StartsWith("rank"))).ToLower();
        ModInfo? mod = ObjectCaching.GetCachedObject<ModInfo>(modName + "_modobj")
            ?? await ExternalAPIHandler.GetModInfo(modName);
        if (mod is null)
        {
            MessageHandler.SendMessage(channel, $"@{user}, An error occured with your request :(");
            return;
        }

        int level = Options.ParseInt("rank", ctx.IrcMessage.Message) ?? mod.fusionLimit;
        if (level > mod.fusionLimit) level = mod.fusionLimit;
        string modString = $"{mod.type} \"{mod.name}\" " +
            $"-- [Rank:{level}/{mod.fusionLimit}] drain:{mod.baseDrain + level} " +
            $"-- {string.Join(" | ", mod.levelStats[level].stats)} ";

        MessageHandler.SendMessage(channel, $"@{user}, {modString}");
        ObjectCaching.CacheObject(modName + "_modobj", mod, 150);
    }
}
