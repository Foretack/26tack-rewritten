using CodeHollow.FeedReader;
using Tack.Database;
using Tack.Handlers;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Modules;
internal sealed class FeedsReader : IModule
{
    public string Name => this.GetType().Name;
    public bool Enabled { get; private set; }
    private readonly string _channel = AppConfigLoader.Config.RelayChannel;

    public FeedsReader()
    {
        Enable();
        Time.DoEvery(TimeSpan.FromMinutes(2.5), ReadFeeds);
    }

    private async Task ReadFeeds()
    {
        if (!Enabled) return;

        var subs = await Redis.Cache.GetObjectAsync<IDictionary<string, string>>("rss:subscriptions");
        var latest = await Redis.Cache.FetchObjectAsync<IDictionary<string, string>>("rss:latest",
            () => Task.FromResult(subs));

        foreach (var sub in subs)
        {
            if (!latest.ContainsKey(sub.Key))
            {
                latest.Add(sub.Key, string.Empty);
                await Redis.Cache.SetObjectAsync("rss:latest", latest);
            }
            var feedReadResult = await FeedReader.ReadAsync(sub.Value);
            if (feedReadResult is null) continue;

            Log.Debug("Reading feed {title}", feedReadResult.Title);

            var item = feedReadResult.Items.FirstOrDefault();
            if (item is null) continue;
            if (item.Title == latest[sub.Key]) continue;

            MessageHandler.SendMessage(_channel, $"💡 {item.Title} -- {item.Link}");
            Log.Information("💡 New item from {sub}: {title} -- {link}", sub, item.Title, item.Link);
            latest[sub.Key] = item.Title;
            await Redis.Cache.SetObjectAsync("rss:latest", latest);
        }
    }

    public void Enable()
    {
        Enabled = true;
        Log.Debug("{type} Enabled", typeof(FeedsReader));
    }

    public void Disable()
    {
        Enabled = false;
        Log.Debug("{type} Disabled", typeof(FeedsReader));
    }
}
