namespace Tack.Models;
internal sealed record RSSFeedSubscription(string Link, string PrependText, string[] Channels)
{
    public static implicit operator string(RSSFeedSubscription o) => o.Link;
};
