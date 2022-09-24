using System.Text.Json.Serialization;

namespace Tack.Models;
public sealed record TwitchMessage(
    [property: JsonPropertyName("BadgeInfo")] IReadOnlyList<BadgeInfo> BadgeInfo,
    [property: JsonPropertyName("Bits")] int Bits,
    [property: JsonPropertyName("BitsInDollars")] int BitsInDollars,
    [property: JsonPropertyName("Channel")] string Channel,
    [property: JsonPropertyName("CheerBadge")] object CheerBadge,
    [property: JsonPropertyName("CustomRewardId")] object CustomRewardId,
    [property: JsonPropertyName("EmoteReplacedMessage")] object EmoteReplacedMessage,
    [property: JsonPropertyName("Id")] string Id,
    [property: JsonPropertyName("IsBroadcaster")] bool IsBroadcaster,
    [property: JsonPropertyName("IsFirstMessage")] bool IsFirstMessage,
    [property: JsonPropertyName("IsHighlighted")] bool IsHighlighted,
    [property: JsonPropertyName("IsMe")] bool IsMe,
    [property: JsonPropertyName("IsModerator")] bool IsModerator,
    [property: JsonPropertyName("IsSkippingSubMode")] bool IsSkippingSubMode,
    [property: JsonPropertyName("IsSubscriber")] bool IsSubscriber,
    [property: JsonPropertyName("IsVip")] bool IsVip,
    [property: JsonPropertyName("IsStaff")] bool IsStaff,
    [property: JsonPropertyName("IsPartner")] bool IsPartner,
    [property: JsonPropertyName("Message")] string Message,
    [property: JsonPropertyName("Noisy")] int Noisy,
    [property: JsonPropertyName("RoomId")] string RoomId,
    [property: JsonPropertyName("SubscribedMonthCount")] int SubscribedMonthCount,
    [property: JsonPropertyName("TmiSentTs")] string TmiSentTs,
    [property: JsonPropertyName("ChatReply")] object ChatReply,
    [property: JsonPropertyName("Badges")] IReadOnlyList<Badge> Badges,
    [property: JsonPropertyName("BotUsername")] string BotUsername,
    [property: JsonPropertyName("Color")] Color Color,
    [property: JsonPropertyName("ColorHex")] string ColorHex,
    [property: JsonPropertyName("DisplayName")] string DisplayName,
    [property: JsonPropertyName("EmoteSet")] EmoteSet EmoteSet,
    [property: JsonPropertyName("IsTurbo")] bool IsTurbo,
    [property: JsonPropertyName("UserId")] string UserId,
    [property: JsonPropertyName("Username")] string Username,
    [property: JsonPropertyName("UserType")] int UserType,
    [property: JsonPropertyName("RawIrcMessage")] string RawIrcMessage
);

public sealed record Badge(
    [property: JsonPropertyName("Key")] string Key,
    [property: JsonPropertyName("Value")] string Value
);

public sealed record BadgeInfo(
    [property: JsonPropertyName("Key")] string Key,
    [property: JsonPropertyName("Value")] string Value
);

public sealed record Color(
    [property: JsonPropertyName("R")] int R,
    [property: JsonPropertyName("G")] int G,
    [property: JsonPropertyName("B")] int B,
    [property: JsonPropertyName("A")] int A,
    [property: JsonPropertyName("IsKnownColor")] bool IsKnownColor,
    [property: JsonPropertyName("IsEmpty")] bool IsEmpty,
    [property: JsonPropertyName("IsNamedColor")] bool IsNamedColor,
    [property: JsonPropertyName("IsSystemColor")] bool IsSystemColor,
    [property: JsonPropertyName("Name")] string Name
);

public sealed record Emote(
    [property: JsonPropertyName("Id")] string Id,
    [property: JsonPropertyName("Name")] string Name,
    [property: JsonPropertyName("StartIndex")] int StartIndex,
    [property: JsonPropertyName("EndIndex")] int EndIndex,
    [property: JsonPropertyName("ImageUrl")] string ImageUrl
);

public sealed record EmoteSet(
    [property: JsonPropertyName("Emotes")] IReadOnlyList<Emote> Emotes,
    [property: JsonPropertyName("RawEmoteSetString")] string RawEmoteSetString
);
