using Serilog;
using Tack.Core;
using TwitchLib.Client;

namespace Tack.Nonclass;
public abstract class ChatModule
{
    /// <summary>
    /// The name of the class that is derived from ChatModule
    /// </summary>
    public string Name => this.GetType().Name;
    public bool Enabled { get; protected set; } = true;

    /// <returns>AnonymousClient.Client when true; Otherwise MainClient.Client</returns>
    protected TwitchClient this[bool anonymous] => anonymous ? AnonymousClient.Client : MainClient.Client;
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
