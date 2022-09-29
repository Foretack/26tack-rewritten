using System.Text.Json.Serialization;
using Tack.Nonclass;

namespace Tack.Json;
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
public sealed record CountedItem(
        [property: JsonPropertyName("count")] int Count,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("key")] string Key
    );

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

public sealed record CurrentSortie(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("activation")] DateTime Activation,
        [property: JsonPropertyName("expiry")] DateTime Expiry,
        [property: JsonPropertyName("startString")] string StartString,
        [property: JsonPropertyName("active")] bool Active,
        [property: JsonPropertyName("rewardPool")] string RewardPool,
        [property: JsonPropertyName("variants")] IReadOnlyList<Variant> Variants,
        [property: JsonPropertyName("boss")] string Boss,
        [property: JsonPropertyName("faction")] string Faction,
        [property: JsonPropertyName("factionKey")] string FactionKey,
        [property: JsonPropertyName("expired")] bool Expired,
        [property: JsonPropertyName("eta")] string Eta
    );
public sealed record Variant(
        [property: JsonPropertyName("node")] string Node,
        [property: JsonPropertyName("boss")] string Boss,
        [property: JsonPropertyName("missionType")] string MissionType,
        [property: JsonPropertyName("planet")] string Planet,
        [property: JsonPropertyName("modifier")] string Modifier,
        [property: JsonPropertyName("modifierDescription")] string ModifierDescription
    );

public sealed class CetusCycle : IWorldCycle
{
    [JsonPropertyName("expiry")]
    public DateTime Expiry { get; set; }
    [JsonPropertyName("isDay")]
    public bool IsDay { get; set; }
    public string State => IsDay ? "☀" : "🌙";
    public string QueryString { get; } = "cetusCycle";
}
public sealed class CambionCycle : IWorldCycle
{
    [JsonPropertyName("expiry")]
    public DateTime Expiry { get; set; }
    [JsonPropertyName("active")]
    public string Active { get; set; }
    public string State => Active;
    public string QueryString { get; } = "cambionCycle";
}
public sealed class VallisCycle : IWorldCycle
{
    [JsonPropertyName("expiry")]
    public DateTime Expiry { get; set; }
    public bool isWarm { get; set; }
    public string State => isWarm ? "🔥" : "❄";
    public string QueryString { get; } = "vallisCycle";
}
public sealed class ZarimanCycle : IWorldCycle
{
    [JsonPropertyName("expiry")]
    public DateTime Expiry { get; set; }
    [JsonPropertyName("state")]
    public string State { get; set; }
    public string QueryString { get; } = "zarimanCycle";
}

public sealed record MarketItems(
    [property: JsonPropertyName("payload")] Payload Payload
);
public sealed record Order(
    [property: JsonPropertyName("quantity")] int Quantity,
    [property: JsonPropertyName("creation_date")] DateTime CreationDate,
    [property: JsonPropertyName("visible")] bool Visible,
    [property: JsonPropertyName("user")] User User,
    [property: JsonPropertyName("last_update")] DateTime LastUpdate,
    [property: JsonPropertyName("platinum")] int Platinum,
    [property: JsonPropertyName("order_type")] string OrderType,
    [property: JsonPropertyName("region")] string Region,
    [property: JsonPropertyName("platform")] string Platform,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("mod_rank")] int ModRank
);
public sealed record Payload(
    [property: JsonPropertyName("orders")] IReadOnlyList<Order> Orders
);
public sealed record User(
    [property: JsonPropertyName("ingame_name")] string IngameName,
    [property: JsonPropertyName("last_seen")] DateTime LastSeen,
    [property: JsonPropertyName("reputation")] int Reputation,
    [property: JsonPropertyName("region")] string Region,
    [property: JsonPropertyName("avatar")] string Avatar,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("status")] string Status
);

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

public sealed record InvasionNode(
    [property: JsonPropertyName("attackerReward")] InvasionReward AttackerReward,
    [property: JsonPropertyName("defenderReward")] InvasionReward DefenderReward
);
public sealed record InvasionReward(
    [property: JsonPropertyName("countedItems")] IReadOnlyList<CountedItem> CountedItems
);

public sealed record SteelPathRewards(
        [property: JsonPropertyName("activation")] DateTime Activation,
        [property: JsonPropertyName("expiry")] DateTime Expiry,
        [property: JsonPropertyName("currentReward")] SPReward CurrentReward,
        [property: JsonPropertyName("remaining")] string Remaining,
        [property: JsonPropertyName("rotation")] IReadOnlyList<Rotation> Rotation,
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
public sealed record Rotation(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("cost")] int Cost
);

public sealed record ItemDropData(
        [property: JsonPropertyName("place")] string Place,
        [property: JsonPropertyName("item")] string Item,
        [property: JsonPropertyName("rarity")] string Rarity,
        [property: JsonPropertyName("chance")] double Chance
    );

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

public record Inventory(
        [property: JsonPropertyName("item")] string Item,
        [property: JsonPropertyName("ducats")] int Ducats,
        [property: JsonPropertyName("credits")] int Credits
);
public sealed record VoidTrader(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("activation")] DateTime Activation,
        [property: JsonPropertyName("expiry")] DateTime Expiry,
        [property: JsonPropertyName("startString")] string StartString,
        [property: JsonPropertyName("active")] bool Active,
        [property: JsonPropertyName("character")] string Character,
        [property: JsonPropertyName("location")] string Location,
        [property: JsonPropertyName("inventory")] IReadOnlyList<Inventory> Inventory,
        [property: JsonPropertyName("psId")] string PsId,
        [property: JsonPropertyName("endString")] string EndString
);

public sealed record WarframeItem(
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("name")] string NormalName
);

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
