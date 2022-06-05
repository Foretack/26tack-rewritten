using _26tack_rewritten.core;
using _26tack_rewritten.handlers;
using _26tack_rewritten.interfaces;
using _26tack_rewritten.models;
using Serilog.Events;

namespace _26tack_rewritten.commands.adminset;
internal class Switch : IChatCommand
{
    public Command Info()
    {
        string name = "switch";
        PermissionLevels permission = PermissionLevels.Whitelisted;

        return new Command(name, permission: permission);
    }

    public async Task Run(CommandContext ctx)
    {
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, " ?? ?? ?? ? idiot");
            return;
        }

        switch (args[0])
        {
            case "verbose":
                MessageHandler.SendMessage(channel, $"{MainClient.LogSwitch.MinimumLevel} -> {LogEventLevel.Verbose}");
                MainClient.LogSwitch.MinimumLevel = LogEventLevel.Verbose;
                break;
            case "debug":
                MessageHandler.SendMessage(channel, $"{MainClient.LogSwitch.MinimumLevel} -> {LogEventLevel.Debug}");
                MainClient.LogSwitch.MinimumLevel = LogEventLevel.Debug;
                break;
            case "info":
                MessageHandler.SendMessage(channel, $"{MainClient.LogSwitch.MinimumLevel} -> {LogEventLevel.Information}");
                MainClient.LogSwitch.MinimumLevel = LogEventLevel.Information;
                break;
            case "warning":
                MessageHandler.SendMessage(channel, $"{MainClient.LogSwitch.MinimumLevel} -> {LogEventLevel.Warning}");
                MainClient.LogSwitch.MinimumLevel = LogEventLevel.Warning;
                break;
            case "error":
                MessageHandler.SendMessage(channel, $"{MainClient.LogSwitch.MinimumLevel} -> {LogEventLevel.Error}");
                MainClient.LogSwitch.MinimumLevel = LogEventLevel.Error;
                break;
            case "fatal":
                MessageHandler.SendMessage(channel, $"{MainClient.LogSwitch.MinimumLevel} -> {LogEventLevel.Fatal}");
                MainClient.LogSwitch.MinimumLevel = LogEventLevel.Fatal;
                break;
            default:
                MessageHandler.SendMessage(channel, $"{MainClient.LogSwitch.MinimumLevel}");
                break;
        }
    }
}
