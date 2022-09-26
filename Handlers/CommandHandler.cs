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
        Log.Verbose($"Loaded Handler {handler.GetType()}");
    }
    #endregion

    #region Handling
    public static async ValueTask HandleCommand(CommandContext ctx)
    {
        string cmdName = ctx.CommandName;
        await Task.Run(() =>
        {
            try
            {
                string prefix = Prefixes.First(x => ctx.IrcMessage.Message.StartsWith(x));
                bool s = Handlers.TryGetValue(prefix, out ChatCommandHandler? handler);
                if (!s || handler is null) return;

                if (CommandList.Info.Aliases.Contains(cmdName.Replace(prefix, string.Empty)))
                {
                    var ctx2 = new CommandContext(ctx.IrcMessage, ctx.Args, prefix, ctx.Permission);
                    CommandList.Run(ctx2).SafeFireAndForget();
                    return;
                }
                if (CommandHelp.Info.Name == cmdName.Replace(prefix, string.Empty))
                {
                    var ctx2 = new CommandContext(ctx.IrcMessage, ctx.Args, prefix, ctx.Permission);
                    CommandHelp.Run(ctx2).SafeFireAndForget();
                    return;
                }

                Command command = handler.Commands.First(kvp => kvp.Key.Contains(cmdName.Replace(prefix, string.Empty))).Value;
                if (!ctx.Permission.Permits(command)) return;
                var cd = new Cooldown(ctx.IrcMessage.Username,
                                           ctx.IrcMessage.Channel,
                                           handler.UseUnifiedCooldowns ? handler : command.Info);
                if (!Cooldown.CheckAndHandleCooldown(cd)) return;

                command
                .Execute(ctx)
                .SafeFireAndForget(onException: ex => Log.Error(ex, $"Error running the command \"{cmdName}\""));
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Something went wrong with executing \"{cmdName}\"");
            }
        });
    }
    #endregion
}

public sealed record CommandContext(TwitchMessage IrcMessage, string[] Args, string CommandName, Permission Permission);
