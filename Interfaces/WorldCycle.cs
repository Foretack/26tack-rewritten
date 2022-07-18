namespace Tack.Interfaces;
public interface IWorldCycle
{
    public DateTime Expiry { get; set; }
    public string State { get; }
}
