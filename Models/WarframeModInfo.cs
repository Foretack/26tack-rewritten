using System.Text.Json.Serialization;

namespace Tack.Models;
public sealed record ModInfo(
    [property: JsonPropertyName("baseDrain")] int BaseDrain,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("compatName")] string CompatName,
    [property: JsonPropertyName("fusionLimit")] int FusionLimit,
    [property: JsonPropertyName("imageName")] string ImageName,
    [property: JsonPropertyName("levelStats")] IReadOnlyList<LevelStat> LevelStats,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("polarity")] string Polarity,
    [property: JsonPropertyName("rarity")] string Rarity,
    [property: JsonPropertyName("releaseDate")] string ReleaseDate,
    [property: JsonPropertyName("tradable")] bool Tradable,
    [property: JsonPropertyName("transmutable")] bool Transmutable,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("wikiaThumbnail")] string WikiaThumbnail,
    [property: JsonPropertyName("wikiaUrl")] string WikiaUrl
);
public sealed record LevelStat(
    [property: JsonPropertyName("stats")] IReadOnlyList<string> Stats
);
