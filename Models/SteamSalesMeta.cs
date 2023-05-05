using Tack.Database;
using Tack.Modules;

namespace Tack.Models;

public sealed record SteamSalesMeta(long Latest, List<string> Channels) : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await Redis.Cache.SetObjectAsync($"bot:modules:{nameof(SteamSales)}", this);
    }
};
