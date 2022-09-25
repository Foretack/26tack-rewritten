using Serilog;
using StackExchange.Redis;
using Tack.Utils;

namespace Tack.Database;
internal static class Redis
{
    private static readonly RedisConnection _connection = new("localhost");
    private static readonly Dictionary<RedisChannel, ChannelMessageQueue> _pubsubChannels = new();

    public static async Task<RedisValue> FromRedisAndSet(this string key, RedisValue newValue) => await _connection.Db.StringGetSetAsync(key, newValue);
    public static async Task<RedisValue> FromRedis(this string key) => await _connection.Db.StringGetAsync(key);
    public static async Task<RedisValueWithExpiry> FromRedisWithExpiry(this string key) => await _connection.Db.StringGetWithExpiryAsync(key);
    public static async Task<RedisValue> FromRedisAndSetExpiry(this string key, TimeSpan setExpiry) => await _connection.Db.StringGetSetExpiryAsync(key, setExpiry);
    public static async Task<RedisValue> FromRedisAndSetExpiry(this string key, DateTime setExpiry) => await _connection.Db.StringGetSetExpiryAsync(key, setExpiry);
    public static async Task<RedisValue> RedisSet(this string key, RedisValue value) => await _connection.Db.StringSetAsync(key, value);
    public static async Task<RedisValue> RedisSetExpiring(this string key, RedisValue value, TimeSpan expiry) => await _connection.Db.StringSetAsync(key, value, expiry);
    public static async Task<RedisValue> RedisSetExpiring(this string key, RedisValue value, DateTime expiry) => await _connection.Db.StringSetAsync(key, value, Time.Until(expiry));

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
