using System.Text.Json.Serialization;

namespace Tack.Models;
public sealed record Alert(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("activation")] DateTime Activation,
    [property: JsonPropertyName("expiry")] DateTime Expiry,
    [property: JsonPropertyName("startString")] string StartString,
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("mission")] Mission Mission,
    [property: JsonPropertyName("expired")] bool Expired,
    [property: JsonPropertyName("eta")] string Eta,
    [property: JsonPropertyName("rewardTypes")] IReadOnlyList<string> RewardTypes
);