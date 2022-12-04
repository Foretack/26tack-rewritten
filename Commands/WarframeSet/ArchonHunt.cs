using Tack.Database;
using Tack.Handlers;
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

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;

        //ArchonData archonData = await "warframe:archonhunt".GetOrCreate<ArchonData>(async () =>
        //{
        //    var r = await ExternalAPIHandler.WarframeStatusApi<ArchonData>("archonHunt", timeout: 10);
        //    if (!r.Success)
        //    {
        //        MessageHandler.SendMessage(channel, $"@{user}, ⚠ Request error! {r.Exception.Message}");
        //        return default!;
        //    }
        //    return r.Value;
        //}, true);
        //if (archonData is null || archonData == default(ArchonData)) return;

        var archonDataCache = await Redis.Cache.TryGetObjectAsync<ArchonData>("warframe:archonhunt");
        if (!archonDataCache.keyExists)
        {
            var r = await ExternalAPIHandler.WarframeStatusApi<ArchonData>("archonHunt", timeout: 10);
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, ⚠ Request failed: {r.Exception.Message}");
                return;
            }
            await Redis.Cache.SetObjectAsync("warframe:archonhunt", r.Value, Time.Until(r.Value.Expiry));
            archonDataCache.value = r.Value;
        }
        ArchonData archonData = archonDataCache.value;

        if (Time.HasPassed(archonData.Expiry))
        {
            _ = await Redis.Cache.RemoveAsync("warframe:archonhunt");
            MessageHandler.SendMessage(channel, $"@{user}, Archon hunt information seems to be outdated, try again in later.");
            return;
        }

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
