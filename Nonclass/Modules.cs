﻿using AsyncAwaitBestPractices;
using Tack.Handlers;
using Tack.Models;

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

    protected ChatModule()
    {
        Enable();
    }

    private void OnTwitchMessage(object? sender, Core.OnMessageArgs e)
    {
        OnMessage(e.ChatMessage).SafeFireAndForget(x => Log.Error(x, $"{Name} encountered an issue"));
    }

    protected abstract ValueTask OnMessage(TwitchMessage ircMessage);

    public void Enable()
    {
        Enabled = true;
        MessageHandler.OnTwitchMsg += OnTwitchMessage;
        OnEnabled.Invoke(this);
        Log.Debug($"Enabled module: {Name}");
    }

    public void Disable()
    {
        Enabled = false;
        MessageHandler.OnTwitchMsg -= OnTwitchMessage;
        OnDisabled.Invoke(this);
        Log.Debug($"Disabled module: {Name}");
    }
}
