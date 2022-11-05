using Serilog.Events;
using Tack.Core;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Commands.AdminSet;
internal sealed class Switch : Command
{
    public override CommandInfo Info { get; } = new(
        name: "switch",
        permission: PermissionLevels.Whitelisted
    );

    public override Task Execute(CommandContext ctx)
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
                MessageHandler.SendMessage(channel, $"{Program.LogSwitch.MinimumLevel} -> {LogEventLevel.Verbose}");
                Program.LogSwitch.MinimumLevel = LogEventLevel.Verbose;
                break;
            case "debug":
                MessageHandler.SendMessage(channel, $"{Program.LogSwitch.MinimumLevel} -> {LogEventLevel.Debug}");
                Program.LogSwitch.MinimumLevel = LogEventLevel.Debug;
                break;
            case "info":
                MessageHandler.SendMessage(channel, $"{Program.LogSwitch.MinimumLevel} -> {LogEventLevel.Information}");
                Program.LogSwitch.MinimumLevel = LogEventLevel.Information;
                break;
            case "warning":
                MessageHandler.SendMessage(channel, $"{Program.LogSwitch.MinimumLevel} -> {LogEventLevel.Warning}");
                Program.LogSwitch.MinimumLevel = LogEventLevel.Warning;
                break;
            case "error":
                MessageHandler.SendMessage(channel, $"{Program.LogSwitch.MinimumLevel} -> {LogEventLevel.Error}");
                Program.LogSwitch.MinimumLevel = LogEventLevel.Error;
                break;
            case "fatal":
                MessageHandler.SendMessage(channel, $"{Program.LogSwitch.MinimumLevel} -> {LogEventLevel.Fatal}");
                Program.LogSwitch.MinimumLevel = LogEventLevel.Fatal;
                break;
            default:
                MessageHandler.SendMessage(channel, $"{Program.LogSwitch.MinimumLevel}");
                break;
        }
        return Task.CompletedTask;
    }
}
