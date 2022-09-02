using Serilog;

namespace Tack.Nonclass;
public abstract class ChatModule
{
    public string Name => this.GetType().Name;
    public bool Enabled { get; protected set; } = true;

    protected Action<ChatModule> OnEnabled { get; set; } = _ => { };
    protected Action<ChatModule> OnDisabled { get; set; } = _ => { };

    public void Enable()
    {
        Enabled = true;
        Log.Information($"Enabled module: {Name}");
        OnEnabled.Invoke(this);
    }

    public void Disable()
    {
        Enabled = false;
        Log.Information($"Disabled module: {Name}");
        OnDisabled.Invoke(this);
    }
}
