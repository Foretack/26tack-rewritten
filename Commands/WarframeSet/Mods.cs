using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal sealed class Mods : Command
{
    public override CommandInfo Info { get; } = new(
        name: "modinfo",
        description: "Get the stats of the closest matching mod. Additional options: `rank:number` (highest rank by default)",
        aliases: new string[] { "mod", "mods" },
        userCooldown: 10,
        channelCooldown: 3
    );

    public override async Task Execute(CommandContext ctx)
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

        (bool keyExists, ModInfo value) = await Redis.Cache.TryGetObjectAsync<ModInfo>($"warframe:mod:{modName}");
        if (!keyExists)
        {
            Result<ModInfo> r = await ExternalAPIHandler.WarframeStatusApi<ModInfo>($"mods/{modName}", string.Empty, string.Empty);
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, ⚠ Request failed: {r.Exception.Message}");
                return;
            }

            await Redis.Cache.SetObjectAsync($"warframe:mod:{modName}", r.Value, TimeSpan.FromHours(8));
            value = r.Value;
        }

        ModInfo mod = value;

        int level = Options.ParseInt("rank", ctx.IrcMessage.Message) ?? mod.FusionLimit;
        if (level > mod.FusionLimit)
            level = mod.FusionLimit;
        string modString =
            $"{mod.Type} \"{mod.Name}\" " +
            $"▣ [Rank:{level}/{mod.FusionLimit}] drain:{mod.BaseDrain + level} " +
            $"▣ {string.Join(" | ", mod.LevelStats[level].Stats)} ";

        MessageHandler.SendMessage(channel, $"@{user}, {modString}");
    }
}