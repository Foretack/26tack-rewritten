namespace Tack.Nonclass;

public interface IModule
{
    public string Name { get; }
    public bool Enabled { get; }
    public void Enable();
    public void Disable();
}

public abstract class ChatModule : IModule
{
    /// <summary>
    /// The name of the class that is derived from ChatModule
    /// </summary>
    public string Name => this.GetType().Name;
    public bool Enabled { get; protected set; } = true;

    protected Action<ChatModule> OnEnabled { get; set; } = _ => { };
    protected Action<ChatModule> OnDisabled { get; set; } = _ => { };

    public void Enable()
    {
        Enabled = true;
        Log.Debug($"Enabled module: {Name}");
        OnEnabled.Invoke(this);
    }

    public void Disable()
    {
        Enabled = false;
        Log.Debug($"Disabled module: {Name}");
        OnDisabled.Invoke(this);
    }
}
