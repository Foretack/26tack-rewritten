namespace Tack.Utils;
internal static class Time
{
    public static TimeSpan Until(DateTime time) => time.ToLocalTime() - DateTime.Now;
    public static string UntilString(DateTime time) => (time.ToLocalTime() - DateTime.Now).FormatTimeLeft();
    public static TimeSpan Since(DateTime time) => DateTime.Now - time.ToLocalTime();
    public static string SinceString(DateTime time) => (DateTime.Now - time.ToLocalTime()).FormatTimeLeft();
}
