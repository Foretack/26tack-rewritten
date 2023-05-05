using Tack.Database;
using Tack.Modules;

namespace Tack.Models;

public sealed record SteamSalesMeta(List<string> Channels) : IAsyncDisposable
{
    public long Latest { get; set; }
    public async ValueTask DisposeAsync()
    {
        await Redis.Cache.SetObjectAsync($"bot:modules:{nameof(SteamSales)}", this);
    }
};
