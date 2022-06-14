namespace Tack.Json;

#pragma warning disable CS8618
#pragma warning disable IDE1006

public class TMI
{
    public ChatterData chatters { get; set; }
}

public class ChatterData
{
    public string[] moderators { get; set; }
    public string[] viewers { get; set; }
}
