using System.Text.Json.Serialization;

namespace Tack.Models;
public sealed record Fissure(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("activation")] DateTime Activation,
    [property: JsonPropertyName("expiry")] DateTime Expiry,
    [property: JsonPropertyName("startString")] string StartString,
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("node")] string Node,
    [property: JsonPropertyName("expired")] bool Expired,
    [property: JsonPropertyName("eta")] string Eta,
    [property: JsonPropertyName("missionType")] string MissionType,
    [property: JsonPropertyName("missionKey")] string MissionKey,
    [property: JsonPropertyName("tier")] string Tier,
    [property: JsonPropertyName("tierNum")] int TierNum,
    [property: JsonPropertyName("enemy")] string Enemy,
    [property: JsonPropertyName("enemyKey")] string EnemyKey,
    [property: JsonPropertyName("isStorm")] bool IsStorm,
    [property: JsonPropertyName("isHard")] bool IsHard
);
