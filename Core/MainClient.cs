﻿using Serilog;
using Tack.Handlers;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.Communication.Events;

namespace Tack.Core;

public static class MainClient
{
    public static bool Connected { get; private set; } = false;

    internal static TwitchClient Client { get; private set; } = new TwitchClient();
    public static void Initialize()
    {
        ClientOptions options = new ClientOptions();
        options.MessagesAllowedInPeriod = 150;
        options.ThrottlingPeriod = TimeSpan.FromSeconds(30);

        ReconnectionPolicy policy = new ReconnectionPolicy(10);
        policy.SetMaxAttempts(10);
        options.ReconnectionPolicy = policy;

        WebSocketClient webSocketClient = new WebSocketClient(options);
        Client = new TwitchClient(webSocketClient);
        Client.AutoReListenOnException = true;

        ConnectionCredentials credentials = new ConnectionCredentials(Config.Auth.Username, Config.Auth.AccessToken);
        Client.Initialize(credentials);

        Connect();
    }

    private static void Connect()
    {
        Client.Connect();
        Client.OnConnectionError += (s, e) 
            => Log.Warning($"MainClient encountered a connection error: {e.Error.Message}");
        Client.OnConnected += ClientConnectedEvent;
        Client.OnDisconnected += ClientDisconnectedEvent;
        Client.OnReconnected += async (s, e) 
            => await Reconnect();
    }

    private static void ClientConnectedEvent(object? sender, OnConnectedArgs e)
    {
        Log.Information($"[Main] Connected");
        Connected = true;
    }

    private static void ClientDisconnectedEvent(object? sender, OnDisconnectedEventArgs e)
    {
        Log.Warning($"[Main] Disconnected");
        Connected = false;
        Client.Reconnect();
    }

    private static async Task Reconnect()
    {
        Client.JoinChannel(Config.RelayChannel);
        Client.SendMessage(Config.RelayChannel, $"ppCircle Reconnecting...");
        Log.Information($"MainClient has triggered {typeof(ChannelHandler)} once more!");
        await ChannelHandler.Connect(true);
    }
}