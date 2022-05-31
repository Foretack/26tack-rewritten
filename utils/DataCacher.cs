using Serilog;

namespace _26tack_rewritten.utils;
public abstract class DataCacher<T>
{
    private readonly Dictionary<string, CachedPiece> CachedData = new Dictionary<string, CachedPiece>();

    protected void ClearCachedData() { CachedData.Clear(); }
    protected void CachePiece(string key, T @object, int maxCachingTime) 
    {
        CachedPiece p = new CachedPiece(@object, DateTimeOffset.Now.ToUnixTimeSeconds(), maxCachingTime);
        bool s = CachedData.TryAdd(key, p);
        if (!s) return;
        // Schedule for removal
        Timer? removalScheduler = null;
        removalScheduler = new Timer(state =>
        {
            CachedData.Remove(key);
            Log.Verbose($"removed cached object with key \"{key}\"");
            removalScheduler?.Dispose();
        }, null, maxCachingTime * 1000, Timeout.Infinite);
    }
    protected CachedPiece? GetCachedPiece(string key)
    {
        CachedData.TryGetValue(key, out CachedPiece? piece);
        return piece;
    }

    protected record CachedPiece(T Object, long CachingUnixTime, int MaxCachingTime);
}
