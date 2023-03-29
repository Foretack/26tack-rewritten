namespace Tack.Models;
internal sealed record RssFeedSubscription(string Link, string PrependText, string[] Channels)
{
    public static implicit operator string(RssFeedSubscription o) => o.Link;
};
