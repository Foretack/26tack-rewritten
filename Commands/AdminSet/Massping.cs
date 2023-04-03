using System.Text;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.AdminSet;
internal sealed class Massping : Command
{
    public override CommandInfo Info { get; } = new(
        name: "massping",
        description: " :tf: ",
        aliases: new string[] { "mp" },
        permission: PermissionLevels.Everyone
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.Message.Author.DisplayName;
        string channel = ctx.Message.Channel.Name;
        string[] args = ctx.Args;
        var sb = new StringBuilder();

        if (args.Length == 0)
        {
            await MessageHandler.SendMessage(channel, $"@{user}, channel unspecified");
            return;
        }

        string targetChannel = args[0].ToLower();
        bool mods = false;

        if (channel != AppConfigLoader.Config.RelayChannel || targetChannel == AppConfigLoader.Config.RelayChannel)
        {
            await MessageHandler.SendMessage(channel, "Can't do that in this channel!");
            return;
        }

        if (args.Length > 1 && args[1].ToLower() == "mods")
            mods = true;

        (bool keyExists, ChatterList value) = await Redis.Cache.TryGetObjectAsync<ChatterList>($"twitch:users:{targetChannel}:chatters");
        if (!keyExists)
        {
            Result<ChatterList> res = await ExternalApiHandler.GetInto<ChatterList>($"https://tmi.twitch.tv/group/user/{targetChannel}/chatters");
            if (!res.Success)
            {
                await MessageHandler.SendMessage(channel, $"@{user}, ⚠ Request failed: {res.Exception.Message}");
                return;
            }

            await Redis.Cache.SetObjectAsync($"twitch:users:{targetChannel}:chatters", res.Value, TimeSpan.FromHours(1));
            value = res.Value;
        }

        ChatterList chatterList = value;

        if (mods)
        {
            AppendMods(chatterList.Chatters.Moderators, ref sb);
            await MessageHandler.SendMessage(channel, sb.ToString());
            return;
        }

        AppendViewers(chatterList.Chatters.Viewers, ref sb);
        await MessageHandler.SendMessage(channel, sb.ToString());
    }

    private void AppendMods(IReadOnlyList<string> modList, ref StringBuilder sb)
    {
        var added = new List<string>();
        for (int i = 0; i < 250; i++)
        {
            if (sb.Length >= 475)
                break;
            string mod = modList.Choice();
            if (added.Contains(mod))
                continue;
            _ = sb.Append(mod)
                .Append(' ');
            added.Add(mod);
        }
    }
    private void AppendViewers(IReadOnlyList<string> viewerList, ref StringBuilder sb)
    {
        var added = new List<string>();
        for (int i = 0; i < 250; i++)
        {
            if (sb.Length >= 475)
                break;
            string viewer = viewerList.Choice();
            if (added.Contains(viewer))
                continue;
            _ = sb.Append(viewer)
                .Append(' ');
            added.Add(viewer);
        }
    }
}
