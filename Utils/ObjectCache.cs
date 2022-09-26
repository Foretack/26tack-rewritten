namespace Tack.Utils;
public static class ObjectCache
{
    private static readonly Dictionary<string, object> CachedObjects = new();
    public static void Clear() { CachedObjects.Clear(); }
    public static void Put(string key, object obj, int cacheTime)
    {
        if (cacheTime <= 0) return;
        bool s = CachedObjects.TryAdd(key, obj);
        if (!s) return;
        Log.Debug($"Cached object with key \"{key}\" for {cacheTime}s");

        Timer? remover = null;
        remover = new Timer(callback =>
        {
            _ = CachedObjects.Remove(key);
            Log.Debug($"Removed cached object \"{key}\"");
            remover?.Dispose();
        }, null, cacheTime * 1000, Timeout.Infinite);
    }
    public static T? Get<T>(string key)
    {
        _ = CachedObjects.TryGetValue(key, out object? obj);
        return (T?)obj;
    }
}
