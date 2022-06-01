using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.models;

namespace _26tack_rewritten.commands.adminset;
internal class PartChannel : IChatCommand
{
    public Command Info()
    {
        string name = "part";
        string description = "Part the specified channel";
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
        bool successful = await ChannelHandler.PartChannel(targetChannel);

        if (successful)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Parted from {targetChannel}");
            return;
        }
        MessageHandler.SendMessage(channel, $"@{user}, There may have been errors parting \"{targetChannel}\"");
    }
}
