using Tack.Handlers;
using Tack.Interfaces;
using Tack.Models;
using Tack.Utils;

namespace Tack.Commands.AdminSet;
internal class JoinChannel : IChatCommand
{
    public Command Info() => new(
        name: "join",
        description: "Join the specified channel",
        permission: PermissionLevels.Whitelisted
        );

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
