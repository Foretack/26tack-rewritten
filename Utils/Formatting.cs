namespace Tack.Utils;
internal static class Formatting
{
    public static string FormatException(this Exception exception)
    {
        return $"`{exception.Message}`\n ->" +
                $" \n`{exception.InnerException}`\n ->" +
                $" \n`{exception.StackTrace}`";
    }

    public static string FormatDate(DateTime date) { return $"{date.Year}-{date.Month}-{date.Day}"; }

    public static string FormatTimeLeft(this TimeSpan ts) => ts switch
    {
        { Days: >= 1, Hours: >= 1 }                      => $"{ts:d' day(s) and 'h' hour(s)'}",
        { Days: >= 1, Hours: 0 }                         => $"{ts:d' day(s)'}",
        { Hours: >= 1, Minutes: >= 1, Seconds: >= 1 }    => $"{ts:h'h 'm'm 's's'}",
        { Hours: >= 1, Minutes: >= 1 }                   => $"{ts:h' hour(s) and 'm' minute(s)'}",
        { Hours: >= 1, Seconds: >= 1 }                   => $"{ts:h' hour(s) and 's' second(s)'}",
        { Hours: >= 1 }                                  => $"{ts:h' hour(s)'}",
        { Minutes: >= 1, Seconds: >= 1}                  => $"{ts:m' minute(s) and 's' second(s)'}",
        { Minutes: >= 1 }                                => $"{ts:m' minute(s)'}",
        { Seconds: >= 1 }                                => $"{ts:s' second(s)'}",
        _                                                => throw new NotImplementedException()
    };

    public static string StripDescriminator(this string str)
    {
        int dCount = str.Count(x => x == '#');

        // No descriminator
        if (dCount == 0) return str;
        // Descriminator at the end, if the last 4 chars are numbers
        if (int.TryParse(str[^4..], out _)) return str[..^3]; // Return the string before the last '#', not including it

        // If someone's name has a '#' and there isn't a descriminator for whatever reason
        return str;
    }

    public static string StripSymbols(this string str)
    {
        char nothing = '\0';
        return str
            .Replace('*', nothing)
            .Replace('_', nothing)
            .Replace('|', nothing);
    }

    public static string ShortenSource(this string str)
    {
        if (str.StartsWith("channel:"))
        {
            return "#" + str.Split(':')[1];
        }
        if (str.StartsWith("discord_channel:"))
        {
            return "###" + str.Split(':')[1];
        }
        if (str.StartsWith("internal:"))
        {
            return "<I>" + str.Split(':')[1];
        }
        return "@" + str.Split(':')[1];
    }
}
