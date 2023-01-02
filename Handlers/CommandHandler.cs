using AsyncAwaitBestPractices;
using Tack.Commands;
using Tack.Models;
using Tack.Nonclass;

namespace Tack.Handlers;
internal static class CommandHandler
{
    #region Properties
    public static Dictionary<string, ChatCommandHandler> Handlers { get; } = new Dictionary<string, ChatCommandHandler>();
    public static List<string> Prefixes { get; } = new List<string>();
    #endregion

    #region Initialization
    public static void Initialize()
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
    public static void HandleCommand(CommandContext ctx)
    {
        string cmdName = ctx.CommandName;
        string prefix = Prefixes.First(x => ctx.IrcMessage.Message.StartsWith(x));

        if (!Handlers.TryGetValue(prefix, out var handler)) return;
        if (handler is null) return;

        if (CommandList.Info.Aliases.Contains(cmdName[prefix.Length..]))
        {
            var newContext = new CommandContext(ctx.IrcMessage, ctx.Args, prefix, ctx.Permission);
            CommandList.Run(newContext).SafeFireAndForget();
        }
        if (CommandHelp.Info.Aliases.Contains(cmdName[prefix.Length..]))
        {
            var newContext = new CommandContext(ctx.IrcMessage, ctx.Args, prefix, ctx.Permission);
            CommandHelp.Run(newContext).SafeFireAndForget();
            return;
        }

        if (!handler.Commands.Any(x => x.Key.Contains(cmdName[prefix.Length..]))) return;
        var command = handler.Commands.First(x => x.Key.Contains(cmdName[prefix.Length..])).Value;

        if (!ctx.Permission.Permits(command)) return;

        var cooldown = new Cooldown(
            ctx.IrcMessage.Username,
            ctx.IrcMessage.Channel,
            handler.UseUnifiedCooldowns ? handler : command.Info);
        if (!Cooldown.CheckAndHandleCooldown(cooldown)) return;

        command.Execute(ctx).SafeFireAndForget(ex => Log.Error(ex, "Error running the command \"{cmdName}\"", cmdName));
    }
    #endregion
}

public sealed record CommandContext(TwitchMessage IrcMessage, string[] Args, string CommandName, Permission Permission);
