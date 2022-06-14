namespace Tack.Utils;
public static class Options
{
    public static string? ParseString(string paramName, string message, string seperator = ":", char splitter = ' ')
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

    public static int? ParseInt(string paramName, string message, string seperator = ":", char splitter = ' ')
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

    public static bool? ParseBool(string paramName, string message, string seperator = ":", char splitter = ' ')
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
