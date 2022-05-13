using _26tack_rewritten.interfaces;
using _26tack_rewritten.models;
using AsyncAwaitBestPractices;
using Serilog;
using TwitchLib.Client.Models;

namespace _26tack_rewritten.handlers;
internal static class CommandHandler
{
    public static readonly Dictionary<string[], IChatCommand> Commands = new Dictionary<string[], IChatCommand>();

    public static void Initialize()
    {
        //
    }

    public static void AddCommand(IChatCommand command)
    {
        List<string> keys = new List<string>(command.Info().Aliases);
        keys.Add(command.Info().Name);
        Commands.Add(keys.ToArray(), command);
    }

    public static void HandleCommand(CommandContext ctx)
    {
        string cmdName = ctx.CommandName;
        IChatCommand command = Commands.First(x => x.Key.Contains(cmdName)).Value;
        Cooldown cd = new Cooldown(ctx.IrcMessage.Username, ctx.IrcMessage.Channel, command.Info());
        if (!Cooldown.CheckAndHandleCooldown(cd)) return;
        command.Run(ctx).SafeFireAndForget(onException: ex => Log.Error(ex, "Command execution failed"));
    }
}

public record CommandContext(ChatMessage IrcMessage, string[] Args, string CommandName, Permission Permission);
