using System.Text.Json.Serialization;

namespace Tack.Models;
public record DiscordPresence(
    [property: JsonPropertyName("Type")] string Type,
    [property: JsonPropertyName("Activities")] IReadOnlyList<Activity> Activities,
    [property: JsonPropertyName("Author")] Author Author
);
public record Activity(
        [property: JsonPropertyName("Details")] string Details,
        [property: JsonPropertyName("State")] string State,
        [property: JsonPropertyName("LargeImage")] LargeImage LargeImage,
        [property: JsonPropertyName("LargeImageText")] string LargeImageText,
        [property: JsonPropertyName("SmallImage")] SmallImage SmallImage,
        [property: JsonPropertyName("SmallImageText")] string SmallImageText,
        [property: JsonPropertyName("StartTimestamp")] DateTime StartTimestamp,
        [property: JsonPropertyName("EndTimestamp")] DateTime? EndTimestamp
);

public record LargeImage(
    [property: JsonPropertyName("Id")] string Id,
    [property: JsonPropertyName("Url")] string Url
);

public record SmallImage(
    [property: JsonPropertyName("Id")] string Id,
    [property: JsonPropertyName("Url")] string Url
);