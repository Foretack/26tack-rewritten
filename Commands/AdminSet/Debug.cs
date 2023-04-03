using Tack.Core;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Commands.AdminSet;
internal sealed class Debug : Command
{
    public override CommandInfo Info { get; } = new(
        name: "debug",
        description: "command for testing and stuff!",
        aliases: new[] { "d" },
        permission: PermissionLevels.Whitelisted
    );

    public override async Task Execute(CommandContext ctx)
    {
        string user = ctx.Message.Author.DisplayName;
        string channel = ctx.Message.Channel.Name;
        string[] args = ctx.Args;
        DbQueries db = new SingleOf<DbQueries>();

        if (args.Length == 0)
            return;

        switch (args[0])
        {
            case "throw":
                string message = "none";
                if (args.Length == 2)
                    message = string.Join(" ", args[1..]);
                bool s = await db.LogException(new TestException(message));
                await MessageHandler.SendMessage(channel, s.ToString());
                break;
            case "restart":
                Program.RestartProcess($"manual restart from user `{user}`");
                break;
            case "pull":
                string output = await Program.GitPull() ?? "Command execution failed, check console :(";
                await MessageHandler.SendMessage(channel, output);
                break;
            case "triggers":
            case "reloadtriggers":
                await MessageHandler.ReloadDiscordTriggers();
                break;
            case "printarr":
                await MessageHandler.SendMessage(channel, ctx.Message.Content.Split(' ').AsString());
                break;
            case "channels":
            case "channelsize":
                int size = ChannelHandler.FetchedChannels.Count;
                await MessageHandler.SendMessage(channel, size.ToString());
                break;
            case "channel":
                if (args.Length < 2)
                {
                    await MessageHandler.SendMessage(channel, "specify channel");
                    return;
                }

                string targetChannel = args[1];
                ExtendedChannel? echannel = await db.GetExtendedChannel(targetChannel);
                await MessageHandler.SendMessage(channel, $"{echannel}");
                break;
            case "properties":
                if (args.Length < 2)
                {
                    await MessageHandler.SendMessage(channel, "specify type");
                    return;
                }

                var t = Type.GetType(args[1]);
                if (t is null)
                {
                    await MessageHandler.SendMessage(channel, "type not found");
                    return;
                }

                IEnumerable<string> properties = t.GetProperties().Select(x =>
                $"{(x is { CanRead: true, CanWrite: false } ? "(Readonly)" : string.Empty)} " +
                $"{(x.GetMethod is not null && x.GetMethod.IsStatic ? "(Static)" : string.Empty)} " +
                $"{x.GetMethod?.Name} -> {x.GetMethod?.ReturnType}");
                await MessageHandler.SendMessage(channel, properties.Join(" | "));
                break;
            case "fields":
                if (args.Length < 2)
                {
                    await MessageHandler.SendMessage(channel, "specify type");
                    return;
                }

                var t_ = Type.GetType(args[1]);
                if (t_ is null)
                {
                    await MessageHandler.SendMessage(channel, "type not found");
                    return;
                }

                IEnumerable<string> fields = t_.GetFields().Select(x =>
                $"{(x.IsInitOnly ? "(Readonly)" : string.Empty)} " +
                $"{(x.IsStatic ? "(Static)" : string.Empty)} " +
                $"{x.FieldType.Name} {x.Name}");
                await MessageHandler.SendMessage(channel, fields.Join(" | "));
                break;
            case "methods":
                if (args.Length < 2)
                {
                    await MessageHandler.SendMessage(channel, "specify type");
                    return;
                }

                var t__ = Type.GetType(args[1]);
                if (t__ is null)
                {
                    await MessageHandler.SendMessage(channel, "type not found");
                    return;
                }

                IEnumerable<string> methods = t__.GetMethods().Select(x => $"{(x.IsPrivate ? "(Private)" : string.Empty)} {x.Name} -> {x.ReturnType}");
                await MessageHandler.SendMessage(channel, methods.Join(" | "));
                break;
            case "reloadconfig":
                try
                {
                    AppConfigLoader.ReloadConfig();
                    await MessageHandler.SendMessage(channel, "Config reloaded!");
                }
                catch (Exception e)
                {
                    Log.Error("Failed to reload config. {type}: {message}",
                        e.GetType().Name,
                        e.Message);
                    await MessageHandler.SendMessage(channel, "Failed to reload config.");
                }

                break;
            case "modules":
                await MessageHandler.SendMessage(channel, ModulesHandler.ListEnabledModules());
                break;
        }
    }

    public sealed class TestException : Exception
    {
        public TestException() { }
        public TestException(string message) : base(message) { }
        public TestException(string message, Exception inner) : base(message, inner) { }
    }
}
