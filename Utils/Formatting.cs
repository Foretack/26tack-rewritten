namespace Tack.Utils;
internal static class Formatting
{
    public static string FormatException(Exception exception)
    {
        return $"`{exception.StackTrace}`\n ----->" +
                $" \n`{exception.Message}`\n ----->" +
                $" \n`{exception.InnerException}`";
    }
    public static string FormatDate(DateTime date) { return $"{date.Year}-{date.Month}-{date.Day}"; }
}
