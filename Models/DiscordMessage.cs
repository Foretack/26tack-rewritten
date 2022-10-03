using System.Text.Json.Serialization;

namespace Tack.Models;
public sealed record DiscordMessage(
    [property: JsonPropertyName("GuildId")] ulong GuildId,
    [property: JsonPropertyName("GuildName")] string GuildName,
    [property: JsonPropertyName("ChannelId")] ulong ChannelId,
    [property: JsonPropertyName("ChannelName")] string ChannelName,
    [property: JsonPropertyName("Content")] string Content,
    [property: JsonPropertyName("Embeds")] IReadOnlyList<Embed> Embeds,
    [property: JsonPropertyName("Attachments")] IReadOnlyList<Attachment> Attachments,
    [property: JsonPropertyName("Author")] Author Author
);

public sealed record Attachment(
    [property: JsonPropertyName("FileName")] string FileName,
    [property: JsonPropertyName("FileSize")] int FileSize,
    [property: JsonPropertyName("MediaType")] string MediaType,
    [property: JsonPropertyName("Url")] string Url,
    [property: JsonPropertyName("ProxyUrl")] string ProxyUrl,
    [property: JsonPropertyName("Height")] int Height,
    [property: JsonPropertyName("Width")] int Width,
    [property: JsonPropertyName("Ephemeral")] object Ephemeral,
    [property: JsonPropertyName("Id")] object Id,
    [property: JsonPropertyName("CreationTimestamp")] DateTime CreationTimestamp
);

public sealed record Author(
    [property: JsonPropertyName("Name")] string Name,
    [property: JsonPropertyName("Url")] string Url,
    [property: JsonPropertyName("IconUrl")] IconUrl IconUrl,
    [property: JsonPropertyName("ProxyIconUrl")] ProxyIconUrl ProxyIconUrl,
    [property: JsonPropertyName("Username")] string Username,
    [property: JsonPropertyName("Discriminator")] string Discriminator,
    [property: JsonPropertyName("Id")] ulong Id,
    [property: JsonPropertyName("Mention")] string Mention,
    [property: JsonPropertyName("IsBot")] bool IsBot,
    [property: JsonPropertyName("Flags")] object Flags
);

public sealed record EmbedColor(
    [property: JsonPropertyName("HasValue")] bool HasValue,
    [property: JsonPropertyName("Value")] Value Value
);

public sealed record Embed(
    [property: JsonPropertyName("Title")] string Title,
    [property: JsonPropertyName("Type")] string Type,
    [property: JsonPropertyName("Description")] string Description,
    [property: JsonPropertyName("Url")] string Url,
    [property: JsonPropertyName("Timestamp")] DateTime Timestamp,
    [property: JsonPropertyName("Color")] EmbedColor Color,
    [property: JsonPropertyName("Footer")] Footer Footer,
    [property: JsonPropertyName("Image")] Image Image,
    [property: JsonPropertyName("Thumbnail")] Thumbnail Thumbnail,
    [property: JsonPropertyName("Video")] object Video,
    [property: JsonPropertyName("Provider")] object Provider,
    [property: JsonPropertyName("Author")] Author Author,
    [property: JsonPropertyName("Fields")] IReadOnlyList<Field> Fields
);

public sealed record Field(
    [property: JsonPropertyName("Name")] string Name,
    [property: JsonPropertyName("Value")] string Value,
    [property: JsonPropertyName("Inline")] bool Inline
);

public sealed record Footer(
    [property: JsonPropertyName("Text")] string Text,
    [property: JsonPropertyName("IconUrl")] IconUrl IconUrl,
    [property: JsonPropertyName("ProxyIconUrl")] ProxyIconUrl ProxyIconUrl
);

public sealed record IconUrl(
    [property: JsonPropertyName("Type")] int Type
);

public sealed record Image(
    [property: JsonPropertyName("Url")] Url Url,
    [property: JsonPropertyName("ProxyUrl")] ProxyUrl ProxyUrl,
    [property: JsonPropertyName("Height")] int Height,
    [property: JsonPropertyName("Width")] int Width
);

public sealed record ProxyIconUrl(
    [property: JsonPropertyName("Type")] int Type
);

public sealed record ProxyUrl(
    [property: JsonPropertyName("Type")] int Type
);

public sealed record Thumbnail(
    [property: JsonPropertyName("Url")] Url Url,
    [property: JsonPropertyName("ProxyUrl")] ProxyUrl ProxyUrl,
    [property: JsonPropertyName("Height")] int Height,
    [property: JsonPropertyName("Width")] int Width
);

public sealed record Url(
    [property: JsonPropertyName("Type")] int Type
);

public sealed record Value(
    [property: JsonPropertyName("Value")] int Val,
    [property: JsonPropertyName("R")] int R,
    [property: JsonPropertyName("G")] int G,
    [property: JsonPropertyName("B")] int B
);
