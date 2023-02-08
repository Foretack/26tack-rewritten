using System.Text.Json.Serialization;

namespace Tack.Models;
public sealed record SteelPathRewards(
        [property: JsonPropertyName("activation")] DateTime Activation,
        [property: JsonPropertyName("expiry")] DateTime Expiry,
        [property: JsonPropertyName("currentReward")] SPReward CurrentReward,
        [property: JsonPropertyName("remaining")] string Remaining,
        [property: JsonPropertyName("rotation")] IReadOnlyList<SteelPathRotation> Rotation,
        [property: JsonPropertyName("evergreens")] IReadOnlyList<Evergreen> Evergreens,
        [property: JsonPropertyName("incursions")] Incursions Incursions
);
public sealed record SPReward(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("cost")] int Cost
);
public sealed record Evergreen(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("cost")] int Cost
);
public sealed record Incursions(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("activation")] DateTime Activation,
        [property: JsonPropertyName("expiry")] DateTime Expiry,
        [property: JsonPropertyName("startString")] string StartString,
        [property: JsonPropertyName("active")] bool Active
);
public sealed record SteelPathRotation(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("cost")] int Cost
);