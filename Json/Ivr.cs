﻿using System.Text.Json.Serialization;

namespace Tack.Json;
public sealed record IvrUserData(
    [property: JsonPropertyName("banned")] bool Banned,
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("bio")] string Bio,
    [property: JsonPropertyName("chatColor")] string ChatColor,
    [property: JsonPropertyName("logo")] string Logo,
    [property: JsonPropertyName("partner")] bool Partner,
    [property: JsonPropertyName("affiliate")] bool Affiliate,
    [property: JsonPropertyName("bot")] bool Bot,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt,
    [property: JsonPropertyName("chatSettings")] ChatSettings ChatSettings,
    [property: JsonPropertyName("badge")] IReadOnlyList<Badge> Badge,
    [property: JsonPropertyName("roles")] Roles Roles,
    [property: JsonPropertyName("settings")] Settings Settings,
    [property: JsonPropertyName("panels")] IReadOnlyList<Panel> Panels
);
public sealed record Badge(
        [property: JsonPropertyName("setID")] string SetID,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("version")] int Version
    );
public sealed record ChatSettings(
    [property: JsonPropertyName("chatDelayMs")] int ChatDelayMs,
    [property: JsonPropertyName("followersOnlyDurationMinutes")] int FollowersOnlyDurationMinutes,
    [property: JsonPropertyName("slowModeDurationSeconds")] int SlowModeDurationSeconds,
    [property: JsonPropertyName("blockLinks")] bool BlockLinks,
    [property: JsonPropertyName("isSubscribersOnlyModeEnabled")] bool IsSubscribersOnlyModeEnabled,
    [property: JsonPropertyName("isEmoteOnlyModeEnabled")] bool IsEmoteOnlyModeEnabled,
    [property: JsonPropertyName("isFastSubsModeEnabled")] bool IsFastSubsModeEnabled,
    [property: JsonPropertyName("isUniqueChatModeEnabled")] bool IsUniqueChatModeEnabled,
    [property: JsonPropertyName("requireVerifiedAccount")] bool RequireVerifiedAccount,
    [property: JsonPropertyName("rules")] IReadOnlyList<string> Rules
);
public sealed record Panel(
    [property: JsonPropertyName("id")] int Id
);
public sealed record Roles(
    [property: JsonPropertyName("isAffiliate")] bool IsAffiliate,
    [property: JsonPropertyName("isPartner")] bool IsPartner,
    [property: JsonPropertyName("isSiteAdmin")] object IsSiteAdmin,
    [property: JsonPropertyName("isStaff")] object IsStaff
);
public sealed record Settings(
    [property: JsonPropertyName("preferredLanguageTag")] string PreferredLanguageTag,
    [property: JsonPropertyName("channelFeedEnabled")] bool ChannelFeedEnabled
);
