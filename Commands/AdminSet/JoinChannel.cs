﻿using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.AdminSet;
internal sealed class JoinChannel : Command
{
    public override CommandInfo Info { get; } = new(
        name: "join",
        description: "Join the specified channel",
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
        if (ChannelHandler.FetchedChannels.Any(x => x.Username == targetChannel))
        {
            await MessageHandler.SendMessage(channel, $"I'm already in that channel! (Aborted)");
            return;
        }

        int priority = Options.ParseInt("priority", ctx.Message.Content) ?? 0;
        bool logged = Options.ParseBool("logged", ctx.Message.Content) ?? true;

        bool successful = await ChannelHandler.JoinChannel(targetChannel, priority, logged);
        if (successful)
        {
            await MessageHandler.SendMessage(channel, $"Attempted to join {targetChannel}");
            return;
        }

        await MessageHandler.SendMessage(channel, $"@{user}, There was an error trying to join that channel fdm");
    }
}
