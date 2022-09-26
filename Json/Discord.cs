

namespace Tack.Json;

public sealed class DiscordMessage
{
    public string? content { get; set; }
    public Embed[]? embeds { get; set; }
    public string? username { get; set; }
    public string? avatar_url { get; set; }

    public sealed class Embed
    {
        public string? title { get; set; }
        public string? type { get; set; }
        public string? description { get; set; }
        public string? url { get; set; }
        public DateTime? timestamp { get; set; }
        public int? color { get; set; }
        public Footer? footer { get; set; }
        public Image? image { get; set; }
        public Thumbnail? thumbnail { get; set; }
        public Video? video { get; set; }
        public Provider? provider { get; set; }
        public Author? author { get; set; }
        public Field[]? fields { get; set; }


        public sealed class Footer
        {
            public string text { get; set; }
            public string? icon_url { get; set; }
            public string? proxy_icon_url { get; set; }
        }
        public sealed class Image
        {
            public string url { get; set; }
            public string? proxy_url { get; set; }
            public int? height { get; set; }
            public int? width { get; set; }
        }
        public sealed class Thumbnail
        {
            public string url { get; set; }
            public string? proxy_url { get; set; }
            public int? height { get; set; }
            public int? width { get; set; }
        }
        public sealed class Video
        {
            public string? url { get; set; }
            public string? proxy_url { get; set; }
            public int? height { get; set; }
            public int? width { get; set; }
        }
        public sealed class Provider
        {
            public string? name { get; set; }
            public string? url { get; set; }
        }
        public sealed class Author
        {
            public string name { get; set; }
            public string? url { get; set; }
            public string? icon_url { get; set; }
            public string? proxy_icon_url { get; set; }
        }
        public sealed class Field
        {
            public string name { get; set; }
            public string value { get; set; }
            public bool? inline { get; set; }
        }
    }
}
