using Serilog;

namespace _26tack_rewritten.utils;
public static class ObjectCaching
{
    private static readonly Dictionary<string, object> CachedObjects = new Dictionary<string, object>();
    public static void ClearCache() { CachedObjects.Clear(); }
    public static void CacheObject(string key, object obj, int cacheTime)
    {
        bool s = CachedObjects.TryAdd(key, obj);
        Log.Debug($"Cached object with key \"{key}\" for {cacheTime}s");
        if (!s) return;

        Timer? remover = null;
        remover = new Timer(callback =>
        {
            CachedObjects.Remove(key);
            Log.Debug($"Removed cached object \"{key}\"");
            remover?.Dispose();
        }, null, cacheTime * 1000, Timeout.Infinite);
    }
    public static T? GetCachedObject<T>(string key)
    {
        CachedObjects.TryGetValue(key, out object? obj);
        return (T?)obj;
    }
}
