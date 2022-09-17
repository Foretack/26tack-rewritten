namespace Tack.Utils;
internal static class Time
{
    public static TimeSpan Until(DateTime time) => time.ToLocalTime() - DateTime.Now;
    public static string UntilString(DateTime time) => (time.ToLocalTime() - DateTime.Now).FormatTimeLeft();
    public static TimeSpan Since(DateTime time) => DateTime.Now - time.ToLocalTime();
    public static string SinceString(DateTime time) => (DateTime.Now - time.ToLocalTime()).FormatTimeLeft();
    public static void Schedule(Action action, DateTime dueTime) => Schedule(action, Until(dueTime));
    public static void DoEvery(int seconds, Action action) => DoEvery(TimeSpan.FromSeconds(seconds), action);

    public static void Schedule(Action action, TimeSpan dueTime)
    {
        Timer? t = null;
        t = new Timer(async _ => await Task.Run(() =>
        {
            action.Invoke();
            t?.Dispose();
        }),
        null, dueTime, Timeout.InfiniteTimeSpan);
    }

    public static void DoEvery(TimeSpan period, Action action)
    {
        System.Timers.Timer timer = new();
        timer.Interval = period.TotalMilliseconds;
        timer.Enabled = true;
        timer.Elapsed += async (_, _) => await Task.Run(() => action.Invoke());
    }
}
