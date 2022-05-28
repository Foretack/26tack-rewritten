using _26tack_rewritten.handlers;
using _26tack_rewritten.models;
using TwitchLib.Client.Models;

namespace _26tack_rewritten.interfaces;

public interface IChatCommand
{
    public Command Info();
    public Task Run(CommandContext ctx);
}

public abstract class ChatCommandHandler
{
    public Dictionary<string[], IChatCommand> Commands { get; } = new Dictionary<string[], IChatCommand>();
    public string Prefix { get; protected set; } = Config.MainPrefix;
    public bool UseUnifiedCooldowns { get; protected set; } = false;
    public int[] Cooldowns { get; protected set; } = { 5, 15 };

    protected void AddCommand(IChatCommand command)
    {
        List<string> keys = new List<string>(command.Info().Aliases)
        {
            command.Info().Name
        };
        Commands.Add(keys.ToArray(), command);
    }
}

public abstract class OptionsParser
{
    public string? GetStringParam(string paramName, string message, string seperator = ":", char splitter = ' ')
    {
        try
        {
            int i = message.IndexOf(paramName + seperator);

            if (i <= 0) return null;

            string param = message[(i + paramName.Length + seperator.Length)..].Split(splitter)[0];

            return param;
        }
        catch
        {
            return null;
        }
    }

    public int? GetIntParam(string paramName, string message, string seperator = ":", char splitter = ' ')
    {
        try
        {
            int i = message.IndexOf(paramName + seperator);

            if (i <= 0) return null;

            string param = message[(i + paramName.Length + seperator.Length)..].Split(splitter)[0];

            return int.Parse(param);
        }
        catch
        {
            return null;
        }
    }

    public bool? GetBoolParam(string paramName, string message, string seperator = ":", char splitter = ' ')
    {
        try
        {
            int i = message.IndexOf(paramName + seperator);

            if (i <= 0) return null;

            string param = message[(i + paramName.Length + seperator.Length)..].Split(splitter)[0];

            return bool.Parse(param);
        }
        catch
        {
            return null;
        }
    }
}
