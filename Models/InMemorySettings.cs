using CachingFramework.Redis.Contracts;

namespace Bot.Models;

internal class InMemorySettings
{
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public required Dictionary<string, bool> EnabledModules { get; set; }

    public Task ToggleModule(string moduleName)
    {
        EnabledModules[moduleName] = !EnabledModules[moduleName];
        return Cache.SetObjectAsync("bot:settings", this, when: When.Exists);
    }
}
