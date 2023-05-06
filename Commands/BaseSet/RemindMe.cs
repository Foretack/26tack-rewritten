using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Commands.BaseSet;
internal class RemindMe : Command
{
    public override CommandInfo Info { get; } = new(
        name: "remindme",
        description: "Set a reminder when a steam AppId goes on sale"
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.Message.Author.Name;
        string channel = ctx.Message.Channel.Name;
        if (ctx.Args.Length < 1)
        {
            await MessageHandler.SendMessage(channel, $"@{user}, Usage: {AppConfig.BasePrefix}{ctx.CommandName} <AppId>");
            return;
        }

        await using var meta = await Redis.Cache.GetObjectAsync<SteamSalesMeta>(SteamSalesMeta.KeyName);
        if (!long.TryParse(ctx.Args[0], out var appId))
        {
            await MessageHandler.SendMessage(channel, $"@{user}, Invalid AppId");
            return;
        }

        if (!meta.Subs.ContainsKey(appId))
            meta.Subs.Add(appId, new());

        meta.Subs[appId].Add(user);
        await MessageHandler.SendMessage(channel, "You will be sent a reminder when that game goes on sale!");
    }
}
