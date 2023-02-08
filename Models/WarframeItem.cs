using System.Text.Json.Serialization;

namespace Tack.Models;
public sealed record WarframeItem(
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("name")] string NormalName
);