﻿using _26tack_rewritten.handlers;
using Discord;
using Discord.WebSocket;
using Serilog;

namespace _26tack_rewritten.core;
internal static class DiscordClient
{
    internal static DiscordSocketClient Client { get; } = new DiscordSocketClient();

    public static async Task Connect()
    {
        await Client.LoginAsync(TokenType.Bot, Config.Auth.DiscordToken);
        await Client.StartAsync();
        RegisterEvents(Client);
    }

    private static void RegisterEvents(DiscordSocketClient client)
    {
        client.MessageReceived += MessageHandler.OnDiscordMessageReceived;
        client.Connected += OnConnected;
        client.Disconnected += OnDisconnected;
    }

    private static Task OnDisconnected(Exception arg)
    {
        Log.Warning("[Discord] Disconnected");
        return Task.CompletedTask;
    }

    private static Task OnConnected()
    {
        Log.Debug("[Discord] Connected");
        return Task.CompletedTask;
    }
}
