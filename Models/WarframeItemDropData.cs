using System.Text.Json.Serialization;

namespace Tack.Models;
public sealed record ItemDropData(
    [property: JsonPropertyName("place")] string Place,
    [property: JsonPropertyName("item")] string Item,
    [property: JsonPropertyName("rarity")] string Rarity,
    [property: JsonPropertyName("chance")] double Chance
);