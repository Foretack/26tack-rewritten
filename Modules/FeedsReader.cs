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
        Time.DoEvery(TimeSpan.FromMinutes(5), ReadFeeds);
    }

    private async Task ReadFeeds()
    {
        if (!Enabled)
            return;

        Dictionary<string, RSSFeedSubscription> subs = await Redis.Cache.GetObjectAsync<Dictionary<string, RSSFeedSubscription>>("rss:subscriptions");
        Dictionary<string, string> latest = await Redis.Cache.FetchObjectAsync("rss:latest",
            () => Task.FromResult(new Dictionary<string, string>()));

        foreach (KeyValuePair<string, RSSFeedSubscription> sub in subs)
        {
            if (!latest.ContainsKey(sub.Key))
            {
                latest.Add(sub.Key, string.Empty);
                await Redis.Cache.SetObjectAsync("rss:latest", latest);
            }

            Feed? feedReadResult = await FeedReader.ReadAsync(sub.Value.Link);
            if (feedReadResult is null
            || (feedReadResult.LastUpdatedDate is not null 
                && (DateTime.Now - feedReadResult.LastUpdatedDate).Value.TotalHours > 1))
            {
                continue;
            }

            Log.Debug("Reading feed {title}", feedReadResult.Title);

            FeedItem? item = feedReadResult.Items.FirstOrDefault();
            if (item is null)
                continue;
            if (item.Title == latest[sub.Key])
                continue;

            Log.Information("💡 New post from [{origin}]: {title} -- {link}", sub.Key, item.Title, item.Link);
            latest[sub.Key] = item.Title;
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

    private void UpdateSettings()
    {
        Program.Settings.EnabledModules[Name] = Enabled;
    }
}
