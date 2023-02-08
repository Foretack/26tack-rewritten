using System.Text.Json.Serialization;

namespace Tack.Models;
public sealed record CountedItem(
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("key")] string Key
);