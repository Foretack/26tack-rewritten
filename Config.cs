namespace _26tack_rewritten;
public static class Config
{
    public const string Host = "";
    public const string DbUsername = "";
    public const string Password = "";
    public const string DatabaseName = "";
    public const string RelayChannel = "";

    //Retrieved from the database
    public static string MainPrefix { get; set; } = "|>";
    public static int MinimumTimeoutTimeForRelay { get; set; } = 28800;
    public static Authorization Auth { get; set; } = default!;
    public static Links Links { get; set; } = default!;
    public static Discord Discord { get; set; } = default!;
}

public record Authorization(string Username, string AccessToken, string ClientID, string SupibotToken, string DiscordToken);
public record Links(string IvrChannels = "https://logs.ivr.fi/channels");
public record Discord(ulong GuildID, ulong PingsChannelID, ulong NewsChannelID, string DiscordPing);
