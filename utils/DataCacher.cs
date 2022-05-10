namespace _26tack_rewritten.utils;
public abstract class DataCacher<T>
{
    private protected Dictionary<string, CachedPiece> CachedData = new Dictionary<string, CachedPiece>();

    protected void ClearCachedData() { CachedData.Clear(); }
    protected void CachePiece(string key, CachedPiece item) { CachedData.TryAdd(key, item); }
    protected CachedPiece? GetCachedPiece(string key)
    {
        bool s = CachedData.TryGetValue(key, out CachedPiece? piece);
        if (s && DateTimeOffset.Now.ToUnixTimeSeconds() - piece!.CachingUnixTime >= piece.MaxCachingTime) return null;
        return piece;
    }

    protected record CachedPiece(T Object, long CachingUnixTime, int MaxCachingTime);
}
