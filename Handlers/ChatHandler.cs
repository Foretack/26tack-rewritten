using Bot.Enums;
using Bot.Interfaces;
using MiniTwitch.Irc.Models;

namespace Bot.Handlers;

public static class ChatHandler
{
    private static Dictionary<string, IChatCommand> _commands = new();

    public static void Setup()
    {
        MainClient.OnMessage += OnMessage;
        AnonClient.OnMessage += OnMessage;

        LoadCommands();
    }

    private static void LoadCommands()
    {
        Type interfaceType = typeof(IChatCommand);
        foreach(Type type in interfaceType.Assembly.GetTypes().Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface))
        {
            if (Activator.CreateInstance(type) is IChatCommand command)
            {
                _commands.Add(command.Info.Name, command);
                Debug("Loaded command: {CommandName}", command.Info.Name);
            }
        }

        Information("ChatHandler loaded {CommandCount} commands", _commands.Count);
    }

    private static ValueTask OnMessage(Privmsg arg)
    {
        ReadOnlySpan<char> content = arg.Content;
        if (content.Length > Config.Prefix.Length + 1 && content.StartsWith(Config.Prefix, StringComparison.CurrentCulture))
            return HandleCommand(arg);

        return default;
    }

    private static ValueTask HandleCommand(Privmsg message)
    {
        ReadOnlySpan<char> content = message.Content;
        foreach (KeyValuePair<string, IChatCommand> kvp in _commands)
        {
            ReadOnlySpan<char> key = kvp.Key;
            if (content[Config.Prefix.Length..].StartsWith(key) && message.Permits(kvp.Value))
                return kvp.Value.Run(message);
        }

        return default;
    }

    private static bool Permits(this Privmsg message, IChatCommand command)
    {
        CommandPermission level;
        if (WhiteListedUserIds.Contains(message.Author.Id))
            level = CommandPermission.Whitelisted;
        else if (message.Author.IsMod)
            level = CommandPermission.Moderators;
        else if (message.Author.IsVip)
            level = CommandPermission.VIPs;
        else if (message.Author.IsSubscriber)
            level = CommandPermission.Subscribers;
        else if (BlackListedUserIds.Contains(message.Author.Id))
            level = CommandPermission.None;
        else
            level = CommandPermission.Everyone;

        return level >= command.Info.Permission;
    }
}
