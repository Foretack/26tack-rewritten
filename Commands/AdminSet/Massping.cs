﻿using System.Text;
using Tack.Handlers;
using Tack.Interfaces;
using Tack.Json;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.AdminSet;
internal class Massping : IChatCommand
{
    public Command Info()
    {
        string name = "massping";
        string description = " :tf: ";
        string[] aliases = { "mp" };
        PermissionLevels permission = PermissionLevels.Whitelisted;

        return new Command(name, description, aliases, permission: permission);
    }

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;
        StringBuilder sb = new StringBuilder();

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, channel unspecified");
            return;
        }

        string targetChannel = args[0];
        bool mods = false;

        if (args.Length > 1 && args[1].ToLower() == "mods") mods = true;

        // TODO: this is terrible
        try
        {
            TMI? clist;
            var c = ObjectCaching.GetCachedObject<TMI>(targetChannel + "_MP");
            if (c is not null) clist = c;
            else
            {
                clist = await ExternalAPIHandler.GetChannelChatters(targetChannel);
                if (clist is null)
                {
                    MessageHandler.SendMessage(channel, $"@{user}, FeelsDankMan failed to retrieve that channel's chatters");
                    return;
                }
                ObjectCaching.CacheObject(targetChannel + "_MP", clist, 600);
            }
            
            if (mods)
            {
                AppendMods(clist.chatters.moderators, ref sb);
                MessageHandler.SendMessage(channel, sb.ToString());
                return;
            }
            AppendViewers(clist.chatters.viewers, ref sb);
            MessageHandler.SendMessage(channel, sb.ToString());
        }
        catch (Exception)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Fetching users timed out :(");
        }
    }

    private void AppendMods(string[] modList, ref StringBuilder sb)
    {
        List<string> added = new List<string>();
        for (int i = 0; i < 250; i++)
        {
            if (sb.Length >= 475) break;
            string mod = modList.Choice();
            if (added.Contains(mod)) continue;
            sb.Append(mod)
                .Append(' ');
            added.Add(mod);
        }
    }
    private void AppendViewers(string[] viewerList, ref StringBuilder sb)
    {
        List<string> added = new List<string>();
        for (int i = 0; i < 250; i++)
        {
            if (sb.Length >= 475) break;
            string viewer = viewerList.Choice();
            if (added.Contains(viewer)) continue;
            sb.Append(viewer)
                .Append(' ');
            added.Add(viewer);
        }
    }
}