using Serilog.Events;
using Tack.Handlers;
using Tack.Interfaces;
using Tack.Models;
using C = Tack.Core.Core;

namespace Tack.Commands.AdminSet;
internal class Switch : IChatCommand
{
    public Command Info()
    {
        string name = "switch";
        PermissionLevels permission = PermissionLevels.Whitelisted;

        return new Command(name, permission: permission);
    }

    public Task Run(CommandContext ctx)
    {
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0)
        {
            MessageHandler.SendMessage(channel, " ?? ?? ?? ? idiot");
            return Task.CompletedTask;
        }

        switch (args[0])
        {
            case "verbose":
                MessageHandler.SendMessage(channel, $"{C.LogSwitch.MinimumLevel} -> {LogEventLevel.Verbose}");
                C.LogSwitch.MinimumLevel = LogEventLevel.Verbose;
                break;
            case "debug":
                MessageHandler.SendMessage(channel, $"{C.LogSwitch.MinimumLevel} -> {LogEventLevel.Debug}");
                C.LogSwitch.MinimumLevel = LogEventLevel.Debug;
                break;
            case "info":
                MessageHandler.SendMessage(channel, $"{C.LogSwitch.MinimumLevel} -> {LogEventLevel.Information}");
                C.LogSwitch.MinimumLevel = LogEventLevel.Information;
                break;
            case "warning":
                MessageHandler.SendMessage(channel, $"{C.LogSwitch.MinimumLevel} -> {LogEventLevel.Warning}");
                C.LogSwitch.MinimumLevel = LogEventLevel.Warning;
                break;
            case "error":
                MessageHandler.SendMessage(channel, $"{C.LogSwitch.MinimumLevel} -> {LogEventLevel.Error}");
                C.LogSwitch.MinimumLevel = LogEventLevel.Error;
                break;
            case "fatal":
                MessageHandler.SendMessage(channel, $"{C.LogSwitch.MinimumLevel} -> {LogEventLevel.Fatal}");
                C.LogSwitch.MinimumLevel = LogEventLevel.Fatal;
                break;
            default:
                MessageHandler.SendMessage(channel, $"{C.LogSwitch.MinimumLevel}");
                break;
        }
        return Task.CompletedTask;
    }
}
