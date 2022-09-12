using System.Text.Json.Serialization;

namespace Tack.Json;

public sealed record TMI(
    [property: JsonPropertyName("chatter_count")] int ChatterCount,
    [property: JsonPropertyName("chatters")] Chatters Chatters
);

public sealed record Chatters(
        [property: JsonPropertyName("broadcaster")] IReadOnlyList<string> Broadcaster,
        [property: JsonPropertyName("vips")] IReadOnlyList<string> Vips,
        [property: JsonPropertyName("moderators")] IReadOnlyList<string> Moderators,
        [property: JsonPropertyName("viewers")] IReadOnlyList<string> Viewers
    );
