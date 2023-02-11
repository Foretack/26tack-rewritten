using CachingFramework.Redis;
using CachingFramework.Redis.Contracts.Providers;
using CachingFramework.Redis.Serializers;
using StackExchange.Redis;

namespace Tack.Database;
internal sealed class Redis
{
    public static ICacheProviderAsync Cache { get; private set; }
    public static ICollectionProvider Collections { get; private set; }
    public static IPubSubProviderAsync PubSub { get; private set; }

    private static bool _initialized;

    public Redis(string host)
    {
        if (_initialized)
            return;

        var context = new RedisContext(ConnectionMultiplexer.Connect(host), new JsonSerializer());
        Cache = context.Cache;
        Collections = context.Collections;
        PubSub = context.PubSub;
        _initialized = true;
    }
}
