using MiniTwitch.Irc.Models;
using Tack.Core;
using Tack.Models;

namespace Tack.Nonclass;

public interface IModule
{
    public string Name { get; }
    public bool Enabled { get; }
    public void Enable();
    public void Disable();
    public void UpdateSettings();
}

public abstract class ChatModule : Singleton, IModule
{
    /// <summary>
    /// The name of the class that is derived from ChatModule
    /// </summary>
    public string Name => GetType().Name;
    public bool Enabled { get; protected set; } = true;

    protected Action<ChatModule> OnEnabled { get; set; } = _ => { };
    protected Action<ChatModule> OnDisabled { get; set; } = _ => { };

    protected ChatModule() => Enable();

    protected abstract ValueTask OnMessage(Privmsg message);

    public void Enable()
    {
        Enabled = true;
        SingleOf<MainClient>.Obj.Client.OnMessage += OnMessage;
        SingleOf<AnonymousClient>.Obj.Client.OnMessage += OnMessage;
        OnEnabled.Invoke(this);
        UpdateSettings();
        Log.Debug("Enabled module: {name}", Name);
    }

    public void Disable()
    {
        Enabled = false;
        SingleOf<MainClient>.Obj.Client.OnMessage -= OnMessage;
        SingleOf<AnonymousClient>.Obj.Client.OnMessage -= OnMessage;
        OnDisabled.Invoke(this);
        UpdateSettings();
        Log.Debug("Disabled module: {name}", Name);
    }

    public void UpdateSettings()
    {
        Program.Settings.EnabledModules[Name] = Enabled;
    }
}
