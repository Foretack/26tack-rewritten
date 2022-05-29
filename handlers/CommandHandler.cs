using _26tack_rewritten.interfaces;
using _26tack_rewritten.models;
using AsyncAwaitBestPractices;
using Serilog;
using TwitchLib.Client.Models;
using _26tack_rewritten.commands;

namespace _26tack_rewritten.handlers;
internal static class CommandHandler
{
    public static Dictionary<string, ChatCommandHandler> Handlers { get; } = new Dictionary<string, ChatCommandHandler>();

    internal static List<string> Prefixes { get; } = new List<string>();

    public static void Initialize()
    {
        RegisterHandler(new BaseHandler());
    }

    public static void RegisterHandler(ChatCommandHandler handler)
    {
        Handlers.Add(handler.Prefix, handler);
        Prefixes.Add(handler.Prefix);
        Log.Verbose($"Loaded Handler {handler.GetType()}");
    }

    public static async Task HandleCommand(CommandContext ctx)
    {
        string cmdName = ctx.CommandName;
        await Task.Run(() =>
        {
            try
            {
                string prefix = Prefixes.First(x => ctx.IrcMessage.Message.StartsWith(x));
                bool s = Handlers.TryGetValue(prefix, out ChatCommandHandler? handler);
                if (!s || handler is null) return;

                IChatCommand command = handler.Commands.First(kvp => kvp.Key.Contains(cmdName)).Value;
                Cooldown cd = new Cooldown(ctx.IrcMessage.Username,
                                           ctx.IrcMessage.Channel,
                                           handler.UseUnifiedCooldowns ? handler : command.Info());
                if (!Cooldown.CheckAndHandleCooldown(cd)) return;

                command
                .Run(ctx)
                .SafeFireAndForget(onException: ex => Log.Error(ex, $"Error running the command \"{cmdName}\""));
            }
            catch (Exception)
            {
                Log.Error( $"Command \"{cmdName}\" does not exist");
            }
        });
    }
}

public record CommandContext(ChatMessage IrcMessage, string[] Args, string CommandName, Permission Permission);
