using System.Text.Json.Serialization;
using Tack.Nonclass;

namespace Tack.Models;
#pragma warning disable CS8618
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
#pragma warning restore CS8618
