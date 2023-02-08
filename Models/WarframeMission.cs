using System.Text.Json.Serialization;

namespace Tack.Models;
public record Mission(
    [property: JsonPropertyName("reward")] Reward Reward,
    [property: JsonPropertyName("node")] string Node,
    [property: JsonPropertyName("nodeKey")] string NodeKey,
    [property: JsonPropertyName("faction")] string Faction,
    [property: JsonPropertyName("factionKey")] string FactionKey,
    [property: JsonPropertyName("maxEnemyLevel")] int MaxEnemyLevel,
    [property: JsonPropertyName("minEnemyLevel")] int MinEnemyLevel,
    [property: JsonPropertyName("maxWaveNum")] int MaxWaveNum,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("typeKey")] string TypeKey,
    [property: JsonPropertyName("nightmare")] bool Nightmare,
    [property: JsonPropertyName("archwingRequired")] bool ArchwingRequired,
    [property: JsonPropertyName("isSharkwing")] bool IsSharkwing,
    [property: JsonPropertyName("enemySpec")] string EnemySpec,
    [property: JsonPropertyName("levelOverride")] string LevelOverride,
    [property: JsonPropertyName("advancedSpawners")] IReadOnlyList<string> AdvancedSpawners,
    [property: JsonPropertyName("requiredItems")] IReadOnlyList<string> RequiredItems,
    [property: JsonPropertyName("consumeRequiredItems")] bool ConsumeRequiredItems,
    [property: JsonPropertyName("leadersAlwaysAllowed")] bool LeadersAlwaysAllowed,
    [property: JsonPropertyName("levelAuras")] IReadOnlyList<string> LevelAuras,
    [property: JsonPropertyName("description")] string Description
);

public sealed record Reward(
    [property: JsonPropertyName("countedItems")] IReadOnlyList<CountedItem> CountedItems,
    [property: JsonPropertyName("thumbnail")] string Thumbnail,
    [property: JsonPropertyName("color")] int Color,
    [property: JsonPropertyName("credits")] int Credits,
    [property: JsonPropertyName("asString")] string AsString,
    [property: JsonPropertyName("items")] IReadOnlyList<string> Items,
    [property: JsonPropertyName("itemString")] string ItemString
);