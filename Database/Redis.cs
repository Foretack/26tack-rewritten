using System.Text.Json;
using StackExchange.Redis;
using Tack.Utils;

namespace Tack.Database;
internal static class Redis
{
    private static readonly RedisConnection _connection = new($"{AppConfigLoader.Config.RedisHost},password={AppConfigLoader.Config.RedisPass}");
    private static readonly Dictionary<RedisChannel, ChannelMessageQueue> _pubsubChannels = new();

    public static async Task<RedisValue> GetSet(this string key, RedisValue newValue) => await _connection.Db.StringGetSetAsync(key, newValue);
    public static async Task<T?> GetSet<T>(this string key, RedisValue newValue)
    {
        var val = await _connection.Db.StringGetSetAsync(key, newValue);
        if (!val.HasValue || val.IsNull) return default(T);
        return JsonSerializer.Deserialize<T>(val!);
    }
    public static async Task<RedisValue> Get(this string key) => await _connection.Db.StringGetAsync(key);
    public static async Task<T?> Get<T>(this string key)
    {
        var val = await _connection.Db.StringGetAsync(key);
        if (!val.HasValue || val.IsNull) return default(T);
        return JsonSerializer.Deserialize<T>(val!);
    }
    public static async Task<RedisValue> GetEx(this string key, TimeSpan setExpiry) => await _connection.Db.StringGetSetExpiryAsync(key, setExpiry);
    public static async Task<T?> GetEx<T>(this string key, TimeSpan setExpiry)
    {
        var val = await _connection.Db.StringGetSetExpiryAsync(key, setExpiry);
        if (!val.HasValue || val.IsNull) return default(T);
        return JsonSerializer.Deserialize<T>(val!);
    }
    public static async Task<bool> SetKey<T>(this string key, T value) => await _connection.Db.StringSetAsync(key, JsonSerializer.Serialize<T>(value));
    public static async Task<bool> SetExpiringKey<T>(this string key, T value, TimeSpan expiry) => await _connection.Db.StringSetAsync(key, JsonSerializer.Serialize<T>(value), expiry);
    public static async Task<bool> SetExpiringKey<T>(this string key, T value, DateTime expiry) => await _connection.Db.StringSetAsync(key, JsonSerializer.Serialize<T>(value), Time.Until(expiry));
    public static async Task<bool> RemoveKey(this string key) => await _connection.Db.KeyDeleteAsync(key);
    public static async Task<object> SetKeyExpiry(this string key, TimeSpan expiry) => await _connection.Db.KeyExpireAsync(key, expiry);
    public static async Task<object> SetKeyExpiry(this string key, DateTime expiry) => await _connection.Db.KeyExpireAsync(key, expiry);


    public static async Task<T> GetOrCreate<T>(this string key, Func<Task<T>> createFunc, bool setWhenCreate = false, TimeSpan? expiry = null)
    {
        // try getting the value first
        var val = await key.Get<T>();
        if (val is null)
        {
            // if the value doesn't exist, try creating one from createFunc()
            val = await createFunc();
            // exit early if createFunc() returns null to avoid caching it
            if (val is null) return default!;
            // set expiring key if parameter expiry isn't null
            if (setWhenCreate && expiry is not null) await key.SetExpiringKey(val, (TimeSpan)expiry);
            else if (setWhenCreate) await key.SetKey(val);
        }
        return val;
    }
    public static async Task<T> GetOrCreate<T>(this string key, Func<T> createFunc, bool setWhenCreate = false, TimeSpan? expiry = null)
    {
        var val = await key.Get<T>();
        if (val is null)
        {
            val = createFunc();
            if (val is null) return default!;
            if (setWhenCreate && expiry is not null) await key.SetExpiringKey(val, (TimeSpan)expiry);
            else if (setWhenCreate) await key.SetKey(val);
        }
        return val;
    }

    public static ChannelMessageQueue Subscribe(string channel) => HandleSubscribe(channel);
    public static long Publish(RedisChannel channel, RedisValue message) => _connection.Sub.Publish(channel, message);
    public static async Task<long> PublishAsync(RedisChannel channel, RedisValue message) => await _connection.Sub.PublishAsync(channel, message);

    private static ChannelMessageQueue HandleSubscribe(string channel)
    {
        if (!_pubsubChannels.ContainsKey(channel))
        {
            Log.Debug($"created new redis-pubsub subscription \"{channel}\"");
            ChannelMessageQueue queue = _connection.Sub.Subscribe(channel);
            _pubsubChannels.Add(channel, queue);
            return queue;
        }

        return _pubsubChannels[channel];
    }
}
