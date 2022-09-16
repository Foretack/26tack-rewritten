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

        ArchonData? archonData = ObjectCache.Get<ArchonData>(CacheKey);
        if (archonData is null)
        {
            var r = await ExternalAPIHandler.WarframeStatusApi<ArchonData>("archonHunt", timeout: 10);
            if (!r.Success)
            {
                MessageHandler.SendMessage(channel, $"@{user}, ⚠ err! {r.Exception.Message}");
                return;
            }
            archonData = r.Value;
        }

        TimeSpan timeLeft = Time.Until(archonData.Expiry);
        if (timeLeft.TotalMilliseconds < 0 || !archonData.Active)
        {
            MessageHandler.SendMessage(channel, $"@{user}, The data retreived is outdated. Try again later?");
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
            + $" Expires in: {timeLeft.FormatTimeLeft()}";

        MessageHandler.SendMessage(channel, $"@{user}, {archonMessage}");
        ObjectCache.Put(CacheKey, archonData, (int)timeLeft.TotalSeconds);
    }
}
