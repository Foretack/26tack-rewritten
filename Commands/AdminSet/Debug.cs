using Tack.Handlers;
using Tack.Nonclass;
using Tack.Models;
using Tack.Database;
using Tack.Utils;
using C = Tack.Core.Core;

namespace Tack.Commands.AdminSet;
internal class Debug : Command
{
    public override CommandInfo Info { get; } = new(
        name: "debug",
        description: "command for testing stuff! Xdxd",
        permission: PermissionLevels.Whitelisted
    );
    
    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.IrcMessage.DisplayName;
        string channel = ctx.IrcMessage.Channel;
        string[] args = ctx.Args;

        if (args.Length == 0) return;


        switch (args[0])
        {
            case "throw":
                DbQueries db = new DbQueries();
                string message = "none";
                if (args.Length == 2) message = string.Join(" ", args[1..]);
                bool s = await db.LogException(new TestException(message));
                MessageHandler.SendMessage(channel, s.ToString());
                break;
            case "restart":
                C.RestartProcess($"manual restart from user `{user}`");
                break;
            case "pull":
                string output = await C.GitPull() ?? "Command execution failed, check console :(";
                MessageHandler.SendMessage(channel, output);
                break;
            case "monitor":
            case "reloadmonitor":
                StreamMonitor.Stop();
                await ChannelHandler.ReloadFetchedChannels();
                StreamMonitor.Reset();
                StreamMonitor.Start();
                break;
            case "triggers":
            case "reloadtriggers":
                MessageHandler.ReloadDiscordTriggers();
                break;
            case "printarr":
                MessageHandler.SendMessage(channel, ctx.IrcMessage.Message.Split(' ').AsString());
                    break;
        }
    }

    public class TestException : Exception
    {
        public TestException() { }
        public TestException(string message) : base(message) { }
        public TestException(string message, Exception inner) : base(message, inner) { }
    }
}
