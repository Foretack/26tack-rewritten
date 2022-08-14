using System.Text.Json.Serialization;

namespace Tack.Json;

#pragma warning disable CS8618
#pragma warning disable IDE1006

public record TMI(
    [property: JsonPropertyName("chatter_count")] int ChatterCount,
    [property: JsonPropertyName("chatters")] Chatters Chatters
);

public record Chatters(
        [property: JsonPropertyName("broadcaster")] IReadOnlyList<string> Broadcaster,
        [property: JsonPropertyName("vips")] IReadOnlyList<string> Vips,
        [property: JsonPropertyName("moderators")] IReadOnlyList<string> Moderators,
        [property: JsonPropertyName("viewers")] IReadOnlyList<string> Viewers
    );
