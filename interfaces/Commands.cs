using _26tack_rewritten.handlers;
using _26tack_rewritten.models;

namespace _26tack_rewritten.interfaces;

public interface IChatCommand
{
    public Command Info();
    public Task Run(CommandContext ctx);
}

public interface ISubCommand
{
    // TODO
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
