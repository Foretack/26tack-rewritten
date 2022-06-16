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
}
