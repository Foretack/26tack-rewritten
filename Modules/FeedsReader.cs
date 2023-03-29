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
    public string Name => GetType().Name;
    public bool Enabled { get; private set; }

    public FeedsReader(bool enabled)
    {
        if (enabled)
            Enable();
        Time.DoEvery(TimeSpan.FromMinutes(2.5), ReadFeeds);
    }

    private async Task ReadFeeds()
    {
        if (!Enabled)
            return;

        Dictionary<string, RSSFeedSubscription> subs = await Redis.Cache.GetObjectAsync<Dictionary<string, RSSFeedSubscription>>("rss:subscriptions");
        Dictionary<string, List<string>> latest = await Redis.Cache.FetchObjectAsync("rss:latest",
            () => Task.FromResult(new Dictionary<string, List<string>>()));

        foreach (KeyValuePair<string, RSSFeedSubscription> sub in subs)
        {
            if (!latest.ContainsKey(sub.Key))
            {
                latest.Add(sub.Key, new());
                await Redis.Cache.SetObjectAsync("rss:latest", latest);
            }

            Feed? feedReadResult = await FeedReader.ReadAsync(sub.Value.Link);
            if (feedReadResult is null)
            {
                continue;
            }

            Log.Debug("Reading feed {title}", feedReadResult.Title);

            FeedItem? item = feedReadResult.Items.FirstOrDefault();
            if (item is null)
                continue;
            if (latest[sub.Key].Contains(item.Link))
                continue;

            Log.Information("💡 New post from [{origin}]: {title} -- {link}", sub.Key, item.Title, item.Link);

            if (latest[sub.Key].Count >= 50)
                latest[sub.Key].Clear();
            latest[sub.Key].Add($"{item.Title} ({item.Link})");
            await Redis.Cache.SetObjectAsync("rss:latest", latest);

            foreach (string channel in sub.Value.Channels)
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

    public void UpdateSettings()
    {
        Program.Settings.EnabledModules[Name] = Enabled;
    }
}
