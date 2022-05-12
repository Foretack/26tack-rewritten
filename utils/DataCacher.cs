namespace _26tack_rewritten.utils;
public abstract class DataCacher<T>
{
    private readonly Dictionary<string, CachedPiece> CachedData = new Dictionary<string, CachedPiece>();

    protected void ClearCachedData() { CachedData.Clear(); }
    protected void CachePiece(string key, T @object, int maxCachingTime) 
    {
        CachedPiece p = new CachedPiece(@object, DateTimeOffset.Now.ToUnixTimeSeconds(), maxCachingTime);
        CachedData.TryAdd(key, p); 
    }
    protected CachedPiece? GetCachedPiece(string key)
    {
        bool s = CachedData.TryGetValue(key, out CachedPiece? piece);
        if (s && DateTimeOffset.Now.ToUnixTimeSeconds() - piece!.CachingUnixTime >= piece.MaxCachingTime)
        {
            CachedData.Remove(key);
            return null;
        }
        return piece;
    }

    protected record CachedPiece(T Object, long CachingUnixTime, int MaxCachingTime);
}
