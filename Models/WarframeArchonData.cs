using System.Text.Json.Serialization;

namespace Tack.Models;
public sealed record ArchonData(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("activation")] DateTime Activation,
    [property: JsonPropertyName("expiry")] DateTime Expiry,
    [property: JsonPropertyName("startString")] string StartString,
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("rewardPool")] string RewardPool,
    [property: JsonPropertyName("missions")] IReadOnlyList<Mission> Missions,
    [property: JsonPropertyName("boss")] string Boss,
    [property: JsonPropertyName("faction")] string Faction,
    [property: JsonPropertyName("factionKey")] string FactionKey,
    [property: JsonPropertyName("expired")] bool Expired,
    [property: JsonPropertyName("eta")] string Eta
);