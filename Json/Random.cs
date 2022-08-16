using System.Text.Json.Serialization;

namespace Tack.Json;

public record JokeAPI(
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("joke")] string Joke
);
