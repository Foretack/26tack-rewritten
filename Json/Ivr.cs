namespace Tack.Json;

#pragma warning disable CS8618
#pragma warning disable IDE1006

public class Channel
{
    public string userID { get; set; }
    public string name { get; set; }
}

public class JustLogLoggedChannels
{
    public Channel[] channels { get; set; }
}

public class IvrUserData
{
    public bool banned { get; set; }
    public string displayName { get; set; }
    public string login { get; set; }
    public string id { get; set; }
    public string logo { get; set; }
    public DateTime createdAt { get; set; }
}
