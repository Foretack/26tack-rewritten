namespace Tack.Utils;
internal static class Formatting
{
    public static string FormatException(this Exception exception)
    {
        return $"`{exception.StackTrace}`\n ----->" +
                $" \n`{exception.Message}`\n ----->" +
                $" \n`{exception.InnerException}`"
                .Replace("'", "\\'")
                .Replace("%", "\\%");
    }
    public static string FormatDate(DateTime date) { return $"{date.Year}-{date.Month}-{date.Day}"; }

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
}
