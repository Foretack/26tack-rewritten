namespace _26tack_rewritten.json;

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