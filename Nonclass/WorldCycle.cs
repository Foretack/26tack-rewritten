namespace Tack.Nonclass;
public interface IWorldCycle
{
    public DateTime Expiry { get; set; }
    public string State { get; }
    public string QueryString { get; }
}