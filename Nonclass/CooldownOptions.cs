namespace Tack.Nonclass;
public interface ICooldownOptions
{
    public string Name { get; }
    public short UserCooldown { get; }
    public short ChannelCooldown { get; }
}
