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
        string user = ctx.Message.Author.DisplayName;
        string channel = ctx.Message.Channel.Name;

        (bool keyExists, ArchonData value) = await Redis.Cache.TryGetObjectAsync<ArchonData>("warframe:archonhunt");
        if (!keyExists)
        {
            Result<ArchonData> r = await ExternalApiHandler.WarframeStatusApi<ArchonData>("archonHunt", timeout: 10);
            if (!r.Success)
            {
                await MessageHandler.SendMessage(channel, $"@{user}, ⚠ Request failed: {r.Exception.Message}");
                return;
            }

            await Redis.Cache.SetObjectAsync("warframe:archonhunt", r.Value, Time.Until(r.Value.Expiry));
            value = r.Value;
        }

        ArchonData archonData = value;

        if (Time.HasPassed(archonData.Expiry))
        {
            _ = await Redis.Cache.RemoveAsync("warframe:archonhunt");
            await MessageHandler.SendMessage(channel, $"@{user}, Archon hunt information seems to be outdated, try again in later.");
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

        await MessageHandler.SendMessage(channel, $"@{user}, {archonMessage}");
    }
}
