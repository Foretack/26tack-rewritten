using System.Text;
using System.Xml;
using CodeHollow.FeedReader;
using CodeHollow.FeedReader.Parser;
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

        Dictionary<string, RssFeedSubscription> subs = await GetSubscriptions();
        Dictionary<string, List<string>> latest = await GetLatestItems();

        foreach (KeyValuePair<string, RssFeedSubscription> sub in subs)
        {
            if (!latest.ContainsKey(sub.Key))
            {
                latest.Add(sub.Key, new());
                await Redis.Cache.SetObjectAsync("rss:latest", latest);
            }

            Feed? feedReadResult;
            try
            {
                feedReadResult = await FeedReader.ReadAsync(sub.Value.Link);
                if (feedReadResult is null)
                    continue;
            }
            catch (Exception ex) when (ex is XmlException or FeedTypeNotSupportedException)
            {
                Log.Debug(ex, "[{h}] Reading feed [{f}] threw a silent exception: ", nameof(FeedsReader), sub.Key);
                continue;
            }

            Log.Debug("Reading feed {title}", feedReadResult.Title);

            IList<FeedItem> items = feedReadResult.Items;
            foreach (FeedItem item in items)
            {
                if (latest[sub.Key].Contains($"{item.Title} ({item.Link})"))
                    continue;

                Log.Information("💡 New post from [{origin}]: {title} -- {link}", sub.Key, item.Title, item.Link);
                StringBuilder sb = new(sub.Value.PrependText);
                _ = sb.Append(' ')
                    .Append(item.Title);
                if (sub.Value.IncludeLink)
                {
                    _ = sb.Append(' ')
                    .Append("--")
                    .Append(' ')
                    .Append(item.Link);
                }

                foreach (string channel in sub.Value.Channels)
                {
                    await MessageHandler.SendMessage(channel, sb.ToString());
                }

                if (latest[sub.Key].Count >= items.Count * 10)
                    latest[sub.Key] = new List<string>(items.Select(x => $"{x.Title} ({x.Link})"));

                latest[sub.Key].Add($"{item.Title} ({item.Link})");
                await Redis.Cache.SetObjectAsync("rss:latest", latest);
            }
        }
    }

    private static Task<Dictionary<string, RssFeedSubscription>> GetSubscriptions()
    {
        return Redis.Cache.GetObjectAsync<Dictionary<string, RssFeedSubscription>>("rss:subscriptions");
    }

    private static Task<Dictionary<string, List<string>>> GetLatestItems()
    {
        return Redis.Cache.FetchObjectAsync("rss:latest",
            () => Task.FromResult(new Dictionary<string, List<string>>()));
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
