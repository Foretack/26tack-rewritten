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

        Dictionary<string, RssFeedSubscription> subs = await Redis.Cache.GetObjectAsync<Dictionary<string, RssFeedSubscription>>("rss:subscriptions");
        Dictionary<string, List<string>> latest = await Redis.Cache.FetchObjectAsync("rss:latest",
            () => Task.FromResult(new Dictionary<string, List<string>>()));

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

            var items = feedReadResult.Items;
            foreach (var item in items)
            {
                if (item is null)
                    continue;
                if (latest[sub.Key].Contains($"{item.Title} ({item.Link})"))
                    continue;

                Log.Information("💡 New post from [{origin}]: {title} -- {link}", sub.Key, item.Title, item.Link);

                latest[sub.Key].Add($"{item.Title} ({item.Link})");
                await Redis.Cache.SetObjectAsync("rss:latest", latest);
                StringBuilder sb = new(sub.Value.PrependText);
                _ = sb.Append(' ')
                    .Append(item.Title)
                    .Append(' ')
                    .Append("--")
                    .Append(' ')
                    .AppendWhen(sub.Value.IncludeLink, item.Link);
                foreach (string channel in sub.Value.Channels)
                {
                    await MessageHandler.SendMessage(channel, sb.ToString());
                }
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
