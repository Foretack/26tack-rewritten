using System.Text.Json.Serialization;

namespace Tack.Models;
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

public record Inventory(
    [property: JsonPropertyName("item")] string Item,
    [property: JsonPropertyName("ducats")] int Ducats,
    [property: JsonPropertyName("credits")] int Credits
);