using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.json;
using _26tack_rewritten.models;
using _26tack_rewritten.utils;

namespace _26tack_rewritten.commands.warframeset;
internal class Mods : IChatCommand
{
    public Command Info()
    {
        string name = "modinfo";
        string description = "Get the stats of the closest matching mod. Additional options: `rank:number` (0 default)";
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

        int level = Options.ParseInt("rank", ctx.IrcMessage.Message) ?? 0;
        if (level > mod.fusionLimit) level = mod.fusionLimit;
        string modString = $"{mod.type} \"{mod.name}\" [Rank:{level}/{mod.fusionLimit}] " +
            $"-- drain:{mod.baseDrain + level} " +
            $"-- {string.Join(" | ", mod.levelStats[level].stats)} " +
            $"{(mod.tradable ? string.Empty : "(untradable)")}";

        MessageHandler.SendMessage(channel, $"@{user}, {modString}");
        ObjectCaching.CacheObject(modName + "_modobj", mod, 150);
    }
}
