using Tack.Handlers;
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
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, $"@{user}, specify a channel to join fdm");
            return;
        }

        string targetChannel = args[0].ToLower();
        if (!ChannelHandler.FetchedChannels.Any(x => x.Username == targetChannel))
        {
            MessageHandler.SendMessage(channel, $"I'm not in that channel! (Aborted)");
            return;
        }

        bool successful = await ChannelHandler.PartChannel(targetChannel);
        if (successful)
        {
            MessageHandler.SendMessage(channel, $"@{user}, Parted from {targetChannel}");
            return;
        }
        MessageHandler.SendMessage(channel, $"@{user}, There may have been errors parting \"{targetChannel}\"");
    }
}
