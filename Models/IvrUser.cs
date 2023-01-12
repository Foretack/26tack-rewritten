// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
using System.Text.Json.Serialization;

public record ChatSettings(
    [property: JsonPropertyName("chatDelayMs")] int ChatDelayMs,
    [property: JsonPropertyName("followersOnlyDurationMinutes")] object FollowersOnlyDurationMinutes,
    [property: JsonPropertyName("slowModeDurationSeconds")] object SlowModeDurationSeconds,
    [property: JsonPropertyName("blockLinks")] bool BlockLinks,
    [property: JsonPropertyName("isSubscribersOnlyModeEnabled")] bool IsSubscribersOnlyModeEnabled,
    [property: JsonPropertyName("isEmoteOnlyModeEnabled")] bool IsEmoteOnlyModeEnabled,
    [property: JsonPropertyName("isFastSubsModeEnabled")] bool IsFastSubsModeEnabled,
    [property: JsonPropertyName("isUniqueChatModeEnabled")] bool IsUniqueChatModeEnabled,
    [property: JsonPropertyName("requireVerifiedAccount")] bool RequireVerifiedAccount,
    [property: JsonPropertyName("rules")] IReadOnlyList<object> Rules
);

public record LastBroadcast(
    [property: JsonPropertyName("startedAt")] DateTime? StartedAt,
    [property: JsonPropertyName("title")] string Title
);

public record Panel(
    [property: JsonPropertyName("id")] string Id
);

public record Roles(
    [property: JsonPropertyName("isAffiliate")] bool IsAffiliate,
    [property: JsonPropertyName("isPartner")] bool IsPartner,
    [property: JsonPropertyName("isStaff")] object IsStaff
);

public record IvrUser(
    [property: JsonPropertyName("banned")] bool Banned,
    [property: JsonPropertyName("banReason")] string BanReason,
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("bio")] string Bio,
    [property: JsonPropertyName("follows")] int Follows,
    [property: JsonPropertyName("followers")] int Followers,
    [property: JsonPropertyName("profileViewCount")] object ProfileViewCount,
    [property: JsonPropertyName("panelCount")] int PanelCount,
    [property: JsonPropertyName("chatColor")] string ChatColor,
    [property: JsonPropertyName("logo")] string Logo,
    [property: JsonPropertyName("banner")] string Banner,
    [property: JsonPropertyName("verifiedBot")] bool VerifiedBot,
    [property: JsonPropertyName("createdAt")] DateTime? CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime? UpdatedAt,
    [property: JsonPropertyName("emotePrefix")] string EmotePrefix,
    [property: JsonPropertyName("roles")] Roles Roles,
    [property: JsonPropertyName("badges")] IReadOnlyList<object> Badges,
    [property: JsonPropertyName("chatSettings")] ChatSettings ChatSettings,
    [property: JsonPropertyName("stream")] object Stream,
    [property: JsonPropertyName("lastBroadcast")] LastBroadcast LastBroadcast,
    [property: JsonPropertyName("panels")] IReadOnlyList<Panel> Panels
);

