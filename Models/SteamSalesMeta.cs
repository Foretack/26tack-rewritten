using Tack.Database;

namespace Tack.Models;

public sealed record SteamSalesMeta(List<string> Channels) : IAsyncDisposable
{
    public static string KeyName { get; } = $"bot:modules:steamsales";

    public long Latest { get; set; }
    public Dictionary<long, List<string>> Subs { get; set; } = new();
    public async ValueTask DisposeAsync()
    {
        await Redis.Cache.SetObjectAsync(KeyName, this);
    }
};
