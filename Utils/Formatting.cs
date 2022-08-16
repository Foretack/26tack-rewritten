namespace Tack.Utils;
internal static class Formatting
{
    public static string FormatException(this Exception exception)
    {
        return exception.Message;
    }

    public static string AsString<T>(this T[] arr)
    {
        return $"[\"{string.Join("\", \"", arr)}\"]";
    }

    public static string AsString<T>(this List<T> list)
    {
        return $"[\"{string.Join("\", \"", list)}\"]";
    }

    public static string FormatTimeLeft(this TimeSpan time)
    {
        return time switch
        {
            { Days: > 1, Hours: > 1 } => $"{time:d' days and 'h' hours'}",
            { Days: 1, Hours: > 1 } => $"{time:d' day and 'h' hours'}",
            { Days: > 1, Hours: 1 } => $"{time:d' days and 'h' hour'}",

            { Days: > 1 } => $"{time:d' days'}",
            { Days: 1 } => $"{time:d' day'}",

            { Hours: >= 1, Minutes: >= 1, Seconds: >= 1 } => $"{time:h'h 'm'm 's's'}",

            { Hours: > 1, Minutes: > 1 } => $"{time:h' hours and 'm' minutes'}",
            { Hours: 1, Minutes: > 1 } => $"{time:h' hour and 'm' minutes'}",
            { Hours: > 1, Minutes: 1 } => $"{time:h' hours and 'm' minute'}",

            { Hours: > 1, Seconds: > 1 } => $"{time:h' hours and 's' seconds'}",
            { Hours: 1, Seconds: > 1 } => $"{time:h' hour and 's' seconds'}",
            { Hours: > 1, Seconds: 1 } => $"{time:h' hours and 's' second'}",

            { Hours: > 1 } => $"{time:h' hours'}",
            { Hours: 1 } => $"{time:h' hour'}",

            { Minutes: > 1, Seconds: > 1 } => $"{time:m' minutes and 's' seconds'}",
            { Minutes: 1, Seconds: > 1 } => $"{time:m' minute and 's' seconds'}",
            { Minutes: > 1, Seconds: 1 } => $"{time:m' minutes and 's' second'}",

            { Minutes: > 1 } => $"{time:m' minutes'}",
            { Minutes: 1 } => $"{time:m' minute'}",

            { Seconds: > 1 } => $"{time:s' seconds'}",
            { Seconds: 1 } => $"{time:s' second'}",

            { Milliseconds: > 0 } => $"{time.Milliseconds}ms",

            _ => throw new NotImplementedException() // fuck you if you get here
        };
    }

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
            .Replace("||", string.Empty);
    }

    public static string ShortenSource(this string str)
    {
        return str.StartsWith("channel:")
            ? "#" + str.Split(':')[1]
            : str.StartsWith("discord_channel:")
            ? "###" + str.Split(':')[1]
            : str.StartsWith("internal:") ? "<I>" + str.Split(':')[1] : "@" + str.Split(':')[1];
    }

    public static string PluralizeWith(this string str, int num)
    {
        return num == 1 ? $"{num} {str}" : $"{num} {str}s";
    }

    public static string PluralizeOn(this string str, int count)
    {
        return count == 1 ? str : str + 's';
    }

    public static string PluralizeOn(this string str, int count, bool withE)
    {
        return count == 1 && withE ? str : str + "es";
    }

    public static string PluralizeOn(this string str, int count, string specialCase)
    {
        return count == 1 ? str : specialCase;
    }
}
