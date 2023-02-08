using System.Text.Json.Serialization;

namespace Tack.Models;
public sealed record RelicData(
    [property: JsonPropertyName("relics")] IReadOnlyList<Relic> Relics
);
public sealed record Relic(
    [property: JsonPropertyName("tier")] string Tier,
    [property: JsonPropertyName("relicName")] string RelicName,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("rewards")] IReadOnlyList<RelicReward> Rewards
);
public sealed record RelicReward(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("itemName")] string ItemName,
    [property: JsonPropertyName("rarity")] string Rarity,
    [property: JsonPropertyName("chance")] float Chance
);