using CodeHollow.FeedReader;
using Tack.Core;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Nonclass;
using Tack.Utils;

namespace Tack.Modules;
internal sealed class FeedsReader : IModule
{
    public string Name => this.GetType().Name;
    public bool Enabled { get; private set; }

    public FeedsReader(bool enabled)
    {
        if (enabled) Enable();
        Time.DoEvery(TimeSpan.FromMinutes(5), ReadFeeds);
    }

    private async Task ReadFeeds()
    {
        if (!Enabled) return;

        var subs = await Redis.Cache.GetObjectAsync<Dictionary<string, RSSFeedSubscription>>("rss:subscriptions");
        var latest = await Redis.Cache.FetchObjectAsync<Dictionary<string, string>>("rss:latest",
            () => Task.FromResult(new Dictionary<string, string>()));

        foreach (var sub in subs)
        {
            if (!latest.ContainsKey(sub.Key))
            {
                latest.Add(sub.Key, string.Empty);
                await Redis.Cache.SetObjectAsync("rss:latest", latest);
            }
            var feedReadResult = await FeedReader.ReadAsync(sub.Value.Link);
            if (feedReadResult is null) continue;

            Log.Debug("Reading feed {title}", feedReadResult.Title);

            var item = feedReadResult.Items.FirstOrDefault();
            if (item is null) continue;
            if (item.Title == latest[sub.Key]) continue;

            Log.Information("💡 New item from {sub}: {title} -- {link}", sub, item.Title, item.Link);
            latest[sub.Key] = item.Title;
            await Redis.Cache.SetObjectAsync("rss:latest", latest);

            foreach (var channel in sub.Value.Channels)
            {
                MessageHandler.SendMessage(channel, $"{sub.Value.PrependText} {item.Title} -- {item.Link}");
            }
        }
    }

    public void Enable()
    {
        Enabled = true;
        UpdateSettings();
        Log.Debug("Enabled {name}", Name);
    }

    public void Disable()
    {
        Enabled = false;
        UpdateSettings();
        Log.Debug("Disabled {name}", Name);
    }

    private void UpdateSettings()
    {
        Program.Settings.EnabledModules[Name] = Enabled;
    }
}
