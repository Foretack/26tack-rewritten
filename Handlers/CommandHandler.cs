using AsyncAwaitBestPractices;
using MiniTwitch.Irc.Models;
using Tack.Commands;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Handlers;
internal sealed class CommandHandler
{
    #region Properties
    public static Dictionary<string, ChatCommandHandler> Handlers { get; } = new Dictionary<string, ChatCommandHandler>();
    public static List<string> Prefixes { get; } = new List<string>();
    #endregion

    #region Initialization
    public CommandHandler()
    {
        RegisterHandler(new BaseHandler());
        RegisterHandler(new AdminHandler());
        RegisterHandler(new WarframeHandler());
    }

    public static void RegisterHandler(ChatCommandHandler handler)
    {
        Handlers.Add(handler.Prefix, handler);
        Prefixes.Add(handler.Prefix);
        Log.Verbose("Loaded Handler {handlerType}", handler.GetType());
    }
    #endregion

    #region Handling
    public static ValueTask HandleCommand(CommandContext ctx)
    {
        string cmdName = ctx.CommandName;
        string prefix = Prefixes.First(x => ctx.Message.Content.StartsWith(x));
        if (!Handlers.TryGetValue(prefix, out ChatCommandHandler? handler) || handler is null)
            return ValueTask.CompletedTask;

        if (CommandList.Info.Aliases.Contains(cmdName))
        {
            var newContext = new CommandContext(ctx.Message, ctx.Args, prefix, ctx.Permission);
            CommandList.Run(newContext).SafeFireAndForget();
        }

        if (CommandHelp.Info.Aliases.Contains(cmdName))
        {
            var newContext = new CommandContext(ctx.Message, ctx.Args, prefix, ctx.Permission);
            CommandHelp.Run(newContext).SafeFireAndForget();
            return ValueTask.CompletedTask;
        }

        if (!handler.Commands.Any(x => x.Key.Contains(cmdName)))
            return ValueTask.CompletedTask;
        Command command = handler.Commands.First(x => x.Key.Contains(cmdName)).Value;

        if (!ctx.Permission.Permits(command))
            return ValueTask.CompletedTask;

        var cooldown = new Cooldown(
            ctx.Message.Author.Name,
            ctx.Message.Channel.Name,
            handler.UseUnifiedCooldowns ? handler : command.Info);
        if (!Cooldown.CheckAndHandleCooldown(cooldown))
            return ValueTask.CompletedTask;

        command.Execute(ctx).SafeFireAndForget(ex => Log.Error(ex, "Error running the command \"{cmdName}\"", cmdName));
        return ValueTask.CompletedTask;
    }
    #endregion
}

public record struct CommandContext(Privmsg Message, string[] Args, string CommandName, Permission Permission);
