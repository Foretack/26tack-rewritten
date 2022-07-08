using Tack.Handlers;
using Tack.Interfaces;
using Tack.Models;
using Db = Tack.Database.Database;
using C = Tack.Core.Core;

namespace Tack.Commands.AdminSet;
internal class Debug : IChatCommand
{
    public Command Info()
    {
        string name = "debug";
        string description = "command for testing stuff! Xdxd";
        PermissionLevels permission = PermissionLevels.Whitelisted;

        return new Command(name, description, permission: permission);
    }

    public async Task Run(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0) return;

        if (args[0] == "throw")
        {
            Db db = new Db();
            string message = "none";
            if (args.Length == 2) message = string.Join(" ", args[1..]);
            bool s = await db.LogException(new TestException(message));
            MessageHandler.SendMessage(channel, s.ToString());
            return;
        }
        if (args[0] == "restart")
        {
            C.RestartProcess($"manual restart from user `{user}`");
            return;
        }
        if (args[0] == "pull")
        {
            string output = await C.GitPull() ?? "Command execution failed, check console :(";
            MessageHandler.SendMessage(channel, output);
            return;
        }
        if (args[0] == "reloadmonitor")
        {
            StreamMonitor.Stop();
            await ChannelHandler.ReloadFetchedChannels();
            StreamMonitor.Reset();
            StreamMonitor.Start();
            return;
        }
    }

    public class TestException : Exception
    {
        public TestException() { }
        public TestException(string message) : base(message) { }
        public TestException(string message, Exception inner) : base(message, inner) { }
    }
}
