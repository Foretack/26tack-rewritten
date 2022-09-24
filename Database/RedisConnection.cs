using Serilog;
using StackExchange.Redis;

namespace Tack.Database;
internal sealed class RedisConnection
{
    public IDatabaseAsync Db { get; private set; }
    public ISubscriber Sub { get; private set; }

    public RedisConnection(string host)
    {
        var con = ConnectionMultiplexer.Connect(host);
        Log.Debug("Redis connection established");
        Db = con.GetDatabase();
        Sub = con.GetSubscriber();
    }
}
