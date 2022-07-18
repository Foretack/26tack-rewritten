#pragma warning disable CS8618
#pragma warning disable IDE1006

using System.Text.Json.Serialization;
using Tack.Interfaces;

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
    public bool isDay { get; set; }
    public string State { get => isDay ? "☀" : "🌙"; }
}
public class CambionCycle : IWorldCycle
{
    [JsonPropertyName("expiry")]
    public DateTime Expiry { get; set; }
    public string active { get; set; }
    public string State { get => active; }
}
public class VallisCycle : IWorldCycle
{
    [JsonPropertyName("expiry")]
    public DateTime Expiry { get; set; }
    public bool isWarm { get; set; }
    public string State { get => isWarm ? "🔥" : "❄"; }
}
public class ZarimanCycle : IWorldCycle
{
    [JsonPropertyName("expiry")]
    public DateTime Expiry { get; set; }
    [JsonPropertyName("state")]
    public string State { get; set; }
}

public class MarketItems
{
    public Payload payload { get; set; }
}
public class Payload
{
    public Order[] orders { get; set; }
}
public class Order
{
    public int quantity { get; set; }
    public int platinum { get; set; }
    public MarketUser user { get; set; }
    public string order_type { get; set; }
}
public class MarketUser
{
    public string status { get; set; }
}

public class RelicData
{
    public Relic[] relics { get; set; }
}
public class Relic
{
    public string tier { get; set; }
    public string relicName { get; set; }
    public string state { get; set; }
    public RelicReward[] rewards { get; set; }
}
public class RelicReward
{
    public string itemName { get; set; }
    public double chance { get; set; }
}

public class InvasionNode
{
    public InvasionReward attackerReward { get; set; }
    public InvasionReward defenderReward { get; set; }
}
public class InvasionReward
{
    public CountedItem[] countedItems { get; set; }
}

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

public class WarframeNewsObj
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("link")]
    public string Link { get; set; }

    [JsonPropertyName("update")]
    public bool Update { get; set; }
}
