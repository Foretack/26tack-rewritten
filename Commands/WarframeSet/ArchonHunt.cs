using Tack.Database;
using Tack.Handlers;
using Tack.Json;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.WarframeSet;
internal sealed class ArchonHunt : Command
{
    public override CommandInfo Info { get; } = new(
        name: "archon",
        description: "Get the current Archon hunt information"
    );

    private const string CacheKey = "wf:archon";

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        ArchonData archonData = await "warframe:archonhunt".GetOrCreate<ArchonData>(async () =>
        {
            var r = await ExternalAPIHandler.WarframeStatusApi<ArchonData>("archonHunt", timeout: 10);
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, ⚠ Request error! {r.Exception.Message}");
                return default!;
            }
            return r.Value;
        }, true);
        if (archonData is null || archonData == default(ArchonData)) return;
        if (Time.HasPassed(archonData.Expiry))
        {
            await "warframe:archonhunt".RemoveKey();
            MessageHandler.SendMessage(channel, $"@{user}, Archon hunt information seems to be outdates, try again in later.");
            return;
        }
        await "warframe:archonhunt".SetKeyExpiry(Time.Until(archonData.Expiry));

        string bossString = archonData.Boss switch
        {
            "Archon Amar" => "Archon Amar 🐺 ♦",
            "Archon Nira" => "Archon Nira 🐍 🍯",
            "Archon Boreal" => "Archon Boreal 🦉 💎",
            _ => archonData.Boss
        };
        string archonMessage = $"This week's hunt: {bossString} "
            + '['
            + string.Join(" ➜ ", archonData.Missions.Select(x => x.TypeKey))
            + ']'
            + $" Expires in: {Time.UntilString(archonData.Expiry)}";

        MessageHandler.SendMessage(channel, $"@{user}, {archonMessage}");
    }
}
