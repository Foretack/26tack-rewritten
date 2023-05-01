namespace Tack.Models;

internal sealed class TwitchStream
{
    public string Username { get; init; }
    public bool IsOnline { get; set; }
    public string Title { get; set; }
    public string GameName { get; set; }
    public DateTime Started { get; set; }

    public TwitchStream(string username, bool isOnline, string title, string gameName, DateTime started)
    {
        Username = username;
        IsOnline = isOnline;
        Title = title;
        GameName = gameName;
        Started = started;
    }
}