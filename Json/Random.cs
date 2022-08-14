using System.Text.Json.Serialization;

namespace Tack.Json;

#pragma warning disable CS8618
#pragma warning disable IDE1006

public record JokeAPI(
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("joke")] string Joke
);
