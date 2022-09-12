using System.Text.Json.Serialization;

namespace Tack.Json;

public sealed record JokeAPI(
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("joke")] string Joke
);
