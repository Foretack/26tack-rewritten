using System.Text.Json.Serialization;

namespace Tack.Models;
public sealed record WarframeMarketItems(
    [property: JsonPropertyName("payload")] Payload Payload
);
public sealed record Order(
    [property: JsonPropertyName("quantity")] int Quantity,
    [property: JsonPropertyName("creation_date")] DateTime CreationDate,
    [property: JsonPropertyName("visible")] bool Visible,
    [property: JsonPropertyName("user")] WarframeMarketUser User,
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
public sealed record WarframeMarketUser(
    [property: JsonPropertyName("ingame_name")] string IngameName,
    [property: JsonPropertyName("last_seen")] DateTime LastSeen,
    [property: JsonPropertyName("reputation")] int Reputation,
    [property: JsonPropertyName("region")] string Region,
    [property: JsonPropertyName("avatar")] string Avatar,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("status")] string Status
);