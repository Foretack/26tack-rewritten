using System.Text.Json;
using StackExchange.Redis;
using Tack.Utils;

namespace Tack.Database;
internal static class Redis
{
    private static readonly RedisConnection _connection = new("localhost");
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
    public static async Task<bool> RedisSet(this string key, RedisValue value) => await _connection.Db.StringSetAsync(key, JsonSerializer.Serialize(value));
    public static async Task<bool> RedisSetExpiring(this string key, RedisValue value, TimeSpan expiry) => await _connection.Db.StringSetAsync(key, JsonSerializer.Serialize(value), expiry);
    public static async Task<bool> RedisSetExpiring(this string key, RedisValue value, DateTime expiry) => await _connection.Db.StringSetAsync(key, JsonSerializer.Serialize(value), Time.Until(expiry));


    public static async Task<T> GetOrCreate<T>(this string key, Func<Task<T>> createFunc, bool setWhenCreate = false, TimeSpan? expiry = null)
    {
        var val = await key.Get<T>();
        if (val is null)
        {
            val = await createFunc();
            if (setWhenCreate && expiry is not null) await key.RedisSetExpiring(JsonSerializer.Serialize(val), (TimeSpan)expiry);
            else if (setWhenCreate) await key.RedisSet(JsonSerializer.Serialize(val));
        }
        return val;
    }
    public static async Task<T> GetOrCreate<T>(this string key, Func<T> createFunc, bool setWhenCreate = false, TimeSpan? expiry = null)
    {
        var val = await key.Get<T>();
        if (val is null)
        {
            val = createFunc();
            if (setWhenCreate && expiry is not null) await key.RedisSetExpiring(JsonSerializer.Serialize(val), (TimeSpan)expiry);
            else if (setWhenCreate) await key.RedisSet(JsonSerializer.Serialize(val));
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
