using System.Text.Json.Serialization;

namespace Tack.Models;
public sealed record InvasionNode(
    [property: JsonPropertyName("attackerReward")] InvasionReward AttackerReward,
    [property: JsonPropertyName("defenderReward")] InvasionReward DefenderReward
);
public sealed record InvasionReward(
    [property: JsonPropertyName("countedItems")] IReadOnlyList<CountedItem> CountedItems
);
