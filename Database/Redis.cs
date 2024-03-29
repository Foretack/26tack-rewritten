﻿using CachingFramework.Redis;
using CachingFramework.Redis.Contracts.Providers;
using CachingFramework.Redis.Serializers;
using StackExchange.Redis;

namespace Tack.Database;
internal static class Redis
{
    public static ICacheProviderAsync Cache { get; private set; } = default!;
    public static ICollectionProvider Collections { get; private set; } = default!;
    public static IPubSubProviderAsync PubSub { get; private set; } = default!;

    private static bool _initialized;

    public static void Init(string host)
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
