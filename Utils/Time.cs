using System.Runtime.CompilerServices;
using AsyncAwaitBestPractices;

namespace Tack.Utils;
internal static class Time
{
    public static TimeSpan Until(DateTime time)
    {
        return time.ToLocalTime() - DateTime.Now;
    }

    public static string UntilString(DateTime time)
    {
        return (time.ToLocalTime() - DateTime.Now).FormatTimeLeft();
    }

    public static TimeSpan Since(DateTime time)
    {
        return DateTime.Now - time.ToLocalTime();
    }

    public static string SinceString(DateTime time)
    {
        return (DateTime.Now - time.ToLocalTime()).FormatTimeLeft();
    }

    public static void Schedule(Action action, DateTime dueTime)
    {
        Schedule(action, Until(dueTime));
    }

    public static void DoEvery(int seconds, Action action)
    {
        DoEvery(TimeSpan.FromSeconds(seconds), action);
    }

    public static bool HasPassed(DateTime datetime)
    {
        return Time.Until(datetime) <= TimeSpan.Zero;
    }

    public static bool HasPassed(TimeSpan timespan)
    {
        return timespan <= TimeSpan.Zero;
    }

    public static void Schedule(Action action, TimeSpan dueTime)
    {
        if (dueTime.TotalMilliseconds < 0)
        {
            Log.Warning("Attempted to schedule something in the past ({dueTime})", dueTime);
            return;
        }

        Timer? t = null;
        t = new Timer(_ =>
        {
            action.Invoke();
            t?.Dispose();
        },
        null, dueTime, Timeout.InfiniteTimeSpan);
    }
    public static void Schedule(Func<Task> task, TimeSpan dueTime,
    [CallerFilePath] string path = default!,
    [CallerLineNumber] int lineNumber = default)
    {
        if (dueTime.TotalMilliseconds < 0)
        {
            Log.Warning("Attempted to schedule something in the past ({dueTime})", dueTime);
            return;
        }

        Timer? t = null;
        t = new Timer(_ =>
        {
            task.Invoke().SafeFireAndForget(
                ex => Log.Error(ex, "Scheduled task failed at: \n{path}:{num}", path, lineNumber));
            t?.Dispose();
        },
        null, dueTime, Timeout.InfiniteTimeSpan);
    }

    public static void DoEvery(TimeSpan period, Action action)
    {
        if (period.TotalMilliseconds < 0)
        {
            Log.Warning("Attempted to set negative period ({period})", period);
            return;
        }

        System.Timers.Timer timer = new()
        {
            Interval = period.TotalMilliseconds,
            Enabled = true
        };
        timer.Elapsed += (_, _) => Task.Run(() => action.Invoke());
    }
    public static void DoEvery(TimeSpan period, Func<Task> task,
    [CallerFilePath] string path = default!,
    [CallerLineNumber] int lineNumber = default)
    {
        if (period.TotalMilliseconds < 0)
        {
            Log.Warning("Attempted to set negative period ({period})", period);
            return;
        }

        System.Timers.Timer timer = new()
        {
            Interval = period.TotalMilliseconds,
            Enabled = true
        };
        timer.Elapsed += (_, _) => task.Invoke().SafeFireAndForget(
            ex => Log.Error(ex, "Recurring task failed at: \n{path}:{line}", path, lineNumber));
    }
}
