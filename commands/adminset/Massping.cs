using System.Text;
using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.json;
using _26tack_rewritten.models;
using _26tack_rewritten.utils;

namespace _26tack_rewritten.commands.adminset;
internal class Massping : DataCacher<TMI>, IChatCommand
{
    public Command Info()
    {
        string name = "massping";
        string description = " :tf: ";
        string[] aliases = { "mp" };

        return new Command(name, description, aliases, permission: PermissionLevels.Whitelisted);
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

        try
        {
            TMI? clist;
            var c = GetCachedPiece(targetChannel);
            if (c is not null) clist = c.Object;
            else
            {
                clist = await ExternalAPIHandler.GetChannelChatters(targetChannel);
                if (clist is null)
                {
                    MessageHandler.SendMessage(channel, $"@{user}, FeelsDankMan failed to retrieve that channel's chatters");
                    return;
                }
                CachePiece(targetChannel, clist, 600);
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
