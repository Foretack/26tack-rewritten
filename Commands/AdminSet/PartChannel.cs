﻿using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Commands.AdminSet;
internal sealed class PartChannel : Command
{
    public override CommandInfo Info { get; } = new(
        name: "part",
        description: "Part the specified channel",
        permission: PermissionLevels.Whitelisted
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.Message.Author.DisplayName;
        string channel = ctx.Message.Channel.Name;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            await MessageHandler.SendMessage(channel, $"@{user}, specify a channel to join fdm");
            return;
        }

        string targetChannel = args[0].ToLower();
        if (!ChannelHandler.FetchedChannels.Any(x => x.Username == targetChannel))
        {
            await MessageHandler.SendMessage(channel, $"I'm not in that channel! (Aborted)");
            return;
        }

        bool successful = await ChannelHandler.PartChannel(targetChannel);
        if (successful)
        {
            await MessageHandler.SendMessage(channel, $"@{user}, Parted from {targetChannel}");
            return;
        }

        await MessageHandler.SendMessage(channel, $"@{user}, There may have been errors parting \"{targetChannel}\"");
    }
}
