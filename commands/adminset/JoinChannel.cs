﻿using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.models;
using _26tack_rewritten.utils;

namespace _26tack_rewritten.commands.adminset;
internal class JoinChannel : IChatCommand
{
    public Command Info()
    {
        string name = "join";
        string description = "Join the specified channel";
        PermissionLevels permission = PermissionLevels.Whitelisted;

        return new Command(name, description, permission: permission);
    }

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, specify a channel to join fdm");
            return;
        }

        string targetChannel = args[0];
        int priority =  Options.ParseInt("priority", ctx.IrcMessage.Message) ?? 0;
        bool logged = Options.ParseBool("logged", ctx.IrcMessage.Message) ?? true;

        bool successful = await ChannelHandler.JoinChannel(targetChannel, priority, logged);

        if (successful)
        {
            MessageHandler.SendMessage(channel, $"Attempted to join {targetChannel}, it was likely successful");
            return;
        }
        MessageHandler.SendMessage(channel, $"@{user}, There was an error trying to join that channel fdm");
    }
}
