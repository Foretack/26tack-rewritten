using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.models;

namespace _26tack_rewritten.commands.adminset;
internal class JoinChannel : OptionsParser, IChatCommand
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
        int priority =  GetIntParam("priority", ctx.IrcMessage.Message) ?? 0;
        bool logged = GetBoolParam("logged", ctx.IrcMessage.Message) ?? true;

        bool successful = await ChannelHandler.JoinChannel(targetChannel, priority, logged); // FIXME: this shit doesn't work

        if (successful)
        {
            MessageHandler.SendMessage(channel, $"Attempted to join {targetChannel}, it was likely successful");
            return;
        }
        MessageHandler.SendMessage(channel, $"@{user}, There was an error trying to join that channel fdm");
    }
}
