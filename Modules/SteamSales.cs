using System.Text;
using System.Text.RegularExpressions;
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

internal class SteamSales : IModule
{
    private const string RSS_URL = "https://nitter.ivr.fi/steam/rss";

    public string Name => GetType().Name;
    public bool Enabled { get; private set; }

    private readonly Regex _freeWeekend = new(@".*FREE WEEKEND.*Play (?<game>.*) free this weekend and save (?<percent>[0-9]+%) when .*!\n\n(?<link>https://store\.steampowered\.com/app/(?<appId>[0-9]+)/[^/]*/)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private readonly Regex _sale = new(@".*Save (?<upTo>up to )?(?<percent>[0-9]+%) with the (?<saleName>.*)!\n\n(?<link>https://store\.steampowered\.com/sale/[^\n]+)", RegexOptions.ExplicitCapture | RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));
    private readonly Regex _gameDeal = new(@".*Save (?<percent>[0-9]+%) on (?<game>.*)!\n\n(?<link>https://store\.steampowered\.com/app/(?<appId>[0-9]+)/[^/]*/)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));

    public SteamSales(bool enabled)
    {
        if (enabled)
            Enable();

        Time.DoEvery(TimeSpan.FromMinutes(5), Report);
    }

    private async Task Report()
    {
        await using SteamSalesMeta meta = await Redis.Cache.FetchObjectAsync(SteamSalesMeta.KeyName,
            () => Task.FromResult(new SteamSalesMeta(new())));
        Feed feedReadResult;
        try
        {
            feedReadResult = await FeedReader.ReadAsync(RSS_URL);
            if (feedReadResult is null)
                return;
        }
        catch (Exception ex) when (ex is XmlException or FeedTypeNotSupportedException)
        {
            Log.Debug(ex, "[{h}] Reading steam feed threw a silent exception: ", Name);
            return;
        }

        Log.Debug("Reading feed {title}", feedReadResult.Title);
        IEnumerable<FeedItem> items = feedReadResult.Items
                .OrderBy(x => new DateTimeOffset(x.PublishingDate ?? DateTime.MinValue).ToUnixTimeSeconds());
        long appId = 0;
        foreach (FeedItem item in items)
        {
            if (item.PublishingDate is null || item.PublishingDate?.Ticks <= meta.Latest)
                continue;

            meta.Latest = item.PublishingDate!.Value.Ticks;
            Log.Information("🐦 New tweet from steam: {title} -- {link}", item.Title, item.Link);
            StringBuilder sb = new("GabeN ");
            if (_sale.Match(item.Title) is { Success: true } sale)
            {
                _ = sb.Append('[');
                if (sale.Groups.ContainsKey("upTo"))
                    _ = sb.Append($"up to ");

                _ = sb.Append($"-{sale.Groups["percent"]}")
                    .Append(']')
                    .Append($" {sale.Groups["saleName"]}: ")
                    .Append(sale.Groups["link"]);

                if (sale.Groups.ContainsKey("appId"))
                    appId = long.Parse(sale.Groups["appId"].Value);
            }
            else if (_freeWeekend.Match(item.Title) is { Success: true } free)
            {
                _ = sb.Append($"[-{free.Groups["percent"]} & free this weekend] ")
                    .Append($"{free.Groups["game"]}: ")
                    .Append($"{free.Groups["link"]}");

                if (free.Groups.ContainsKey("appId"))
                    appId = long.Parse(free.Groups["appId"].Value);
            }
            else if (_gameDeal.Match(item.Title) is { Success: true } deal)
            {
                _ = sb.Append($"[-{deal.Groups["percent"]}] ")
                    .Append($"{deal.Groups["game"]}: ")
                    .Append($"{deal.Groups["link"]}");

                if (deal.Groups.ContainsKey("appId"))
                    appId = long.Parse(deal.Groups["appId"].Value);
            }
            else
            {
                _ = sb.Append(item.Title);
            }

            if (!Enabled)
                continue;

            foreach (string channel in meta.Channels)
            {
                await MessageHandler.SendMessage(channel, sb.ToString());
                await Task.Delay(250);
            }

            if (!meta.Subs.ContainsKey(appId))
                continue;

            foreach (string user in meta.Subs[appId])
            {
                await MessageHandler.SendMessage("supibot", $"$remind {user} {sb}");
                _ = meta.Subs[appId].Remove(user);
                await Task.Delay(TimeSpan.FromSeconds(15));
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
