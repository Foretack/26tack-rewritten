namespace Tack;
public static class Config
{
    public const string Host = "localhost";
    public const string DbUsername = "postgres";
    public const string Password = "uou12345";
    public const string DatabaseName = "teststuffone";
    public const string RelayChannel = "login_unavailable";

    public static string MainPrefix { get; set; } = "|>";
    public static int MinimumTimeoutTimeForRelay { get; set; } = 28800;
    //Retrieved from the database
    public static Authorization Auth { get; set; } = default!;
    public static Links Links { get; set; } = default!;
    public static Discord Discord { get; set; } = default!;
}

public record Authorization(string Username, string AccessToken, string ClientID, string SupibotToken, string DiscordToken);
public record Links(string IvrChannels = "https://logs.ivr.fi/channels");
public record Discord(ulong GuildID, ulong PingsChannelID, ulong NewsChannelID, string DiscordPing);
