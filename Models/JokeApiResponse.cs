using System.Text.Json.Serialization;

namespace Tack.Models;
public sealed record JokeApiResponse(
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("joke")] string Joke
);