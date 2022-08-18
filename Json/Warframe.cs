
#pragma warning disable CS8618

using System.Text.Json.Serialization;
using Tack.Nonclass;

namespace Tack.Json;
public class Alert
{
    public bool active { get; set; }
    public Mission mission { get; set; }
}
public class Mission
{
    public string description { get; set; }
    public string node { get; set; }
    public string type { get; set; }
    public string faction { get; set; }
    public Reward reward { get; set; }
    public int minEnemyLevel { get; set; }
    public int maxEnemyLevel { get; set; }
    public int maxWaveNum { get; set; }
    public bool nightmare { get; set; }
    public bool archwingRequired { get; set; }
    public bool isSharkwing { get; set; }
    public string levelOverride { get; set; }
    public string enemySpec { get; set; }
    public object[] advancedSpawners { get; set; }
    public object[] requiredItems { get; set; }
    public object[] levelAuras { get; set; }
}
public class Reward
{
    public CountedItem[] countedItems { get; set; }
    public int credits { get; set; }
    public string asString { get; set; }
}
public class CountedItem
{
    public int count { get; set; }
    public string type { get; set; }
    public string key { get; set; }
}

public class Fissure
{
    public bool active { get; set; }
    public string missionType { get; set; }
    public string enemy { get; set; }
    public int tierNum { get; set; }
    public string eta { get; set; }
    public bool isStorm { get; set; }
}

public class CurrentSortie
{
    public DateTime expiry { get; set; }
    public Variant[] variants { get; set; }
    public string boss { get; set; }
    public string faction { get; set; }
    public bool expired { get; set; }
    public string eta { get; set; }
}
public class Variant
{
    public string missionType { get; set; }
    public string modifier { get; set; }
    public string modifierDescription { get; set; }
}

public class CetusCycle : IWorldCycle
{
    [JsonPropertyName("expiry")]
    public DateTime Expiry { get; set; }
    [JsonPropertyName("isDay")]
    public bool IsDay { get; set; }
    public string State => IsDay ? "☀" : "🌙";
    public string QueryString { get; } = "cetusCycle";
}
public class CambionCycle : IWorldCycle
{
    [JsonPropertyName("expiry")]
    public DateTime Expiry { get; set; }
    [JsonPropertyName("active")]
    public string Active { get; set; }
    public string State => Active;
    public string QueryString { get; } = "cambionCycle";
}
public class VallisCycle : IWorldCycle
{
    [JsonPropertyName("expiry")]
    public DateTime Expiry { get; set; }
    public bool isWarm { get; set; }
    public string State => isWarm ? "🔥" : "❄";
    public string QueryString { get; } = "vallisCycle";
}
public class ZarimanCycle : IWorldCycle
{
    [JsonPropertyName("expiry")]
    public DateTime Expiry { get; set; }
    [JsonPropertyName("state")]
    public string State { get; set; }
    public string QueryString { get; } = "zarimanCycle";
}

public record MarketItems(
    [property: JsonPropertyName("payload")] Payload Payload
);
public record Order(
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
public record Payload(
    [property: JsonPropertyName("orders")] IReadOnlyList<Order> Orders
);
public record User(
    [property: JsonPropertyName("ingame_name")] string IngameName,
    [property: JsonPropertyName("last_seen")] DateTime LastSeen,
    [property: JsonPropertyName("reputation")] int Reputation,
    [property: JsonPropertyName("region")] string Region,
    [property: JsonPropertyName("avatar")] string Avatar,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("status")] string Status
);

public record RelicData(
    [property: JsonPropertyName("relics")] IReadOnlyList<Relic> Relics
);
public record Relic(
    [property: JsonPropertyName("tier")] string Tier,
    [property: JsonPropertyName("relicName")] string RelicName,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("rewards")] IReadOnlyList<RelicReward> Rewards,
    [property: JsonPropertyName("_id")] string Id
);
public record RelicReward(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("itemName")] string ItemName,
    [property: JsonPropertyName("rarity")] string Rarity,
    [property: JsonPropertyName("chance")] float Chance
);

public record InvasionNode(
    [property: JsonPropertyName("attackerReward")] InvasionReward AttackerReward,
    [property: JsonPropertyName("defenderReward")] InvasionReward DefenderReward
);
public record InvasionReward(
    [property: JsonPropertyName("countedItems")] IReadOnlyList<CountedItem> CountedItems
);

public class SPReward
{
    public string name { get; set; }
    public int cost { get; set; }
}
public class SteelPathRewards
{
    public DateTime expiry { get; set; }
    public SPReward currentReward { get; set; }
    public SPReward[] rotation { get; set; }
}

public class ItemDropData
{
    public string item { get; set; }
    public float chance { get; set; }
    public string place { get; set; }
}

public class LevelStat
{
    public string[] stats { get; set; }
}
public class ModInfo
{
    public string name { get; set; }
    public string type { get; set; }
    public int baseDrain { get; set; }
    public int fusionLimit { get; set; }
    public LevelStat[] levelStats { get; set; }
}

public class Inventory
{
    [JsonPropertyName("item")]
    public string Item { get; set; }

    [JsonPropertyName("ducats")]
    public int Ducats { get; set; }

    [JsonPropertyName("credits")]
    public int Credits { get; set; }
}
public class VoidTrader
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("activation")]
    public DateTime Activation { get; set; }

    [JsonPropertyName("startString")]
    public string StartString { get; set; }

    [JsonPropertyName("expiry")]
    public DateTime Expiry { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("character")]
    public string Character { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; }

    [JsonPropertyName("inventory")]
    public List<Inventory> Inventory { get; set; }

    [JsonPropertyName("psId")]
    public string PsId { get; set; }

    [JsonPropertyName("endString")]
    public string EndString { get; set; }

    [JsonPropertyName("initialStart")]
    public DateTime InitialStart { get; set; }

    [JsonPropertyName("schedule")]
    public List<object> Schedule { get; set; }
}

public record WarframeItem(
    [property: JsonPropertyName("uniqueName")] string UniqueName,
    [property: JsonPropertyName("name")] string NormalName
);
