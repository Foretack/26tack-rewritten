#pragma warning disable CS8618
#pragma warning disable IDE1006

namespace _26tack_rewritten.json;
public class Alert
{
    public string id { get; set; }
    public DateTime activation { get; set; }
    public string startString { get; set; }
    public DateTime expiry { get; set; }
    public bool active { get; set; }
    public Mission mission { get; set; }
    public string eta { get; set; }
    public string[] rewardTypes { get; set; }
    public string tag { get; set; }
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
    public object[] items { get; set; }
    public Counteditem[] countedItems { get; set; }
    public int credits { get; set; }
    public string asString { get; set; }
    public string itemString { get; set; }
    public string thumbnail { get; set; }
    public int color { get; set; }
}
public class Counteditem
{
    public int count { get; set; }
    public string type { get; set; }
    public string key { get; set; }
}

public class Fissure
{
    public string id { get; set; }
    public DateTime activation { get; set; }
    public string startString { get; set; }
    public DateTime expiry { get; set; }
    public bool active { get; set; }
    public string node { get; set; }
    public string missionType { get; set; }
    public string missionKey { get; set; }
    public string enemy { get; set; }
    public string enemyKey { get; set; }
    public string nodeKey { get; set; }
    public string tier { get; set; }
    public int tierNum { get; set; }
    public bool expired { get; set; }
    public string eta { get; set; }
    public bool isStorm { get; set; }
}