﻿namespace Bot.Models;

#pragma warning disable CS8618
public class AppConfig
{
    public string Username { get; init; }
    public string Token { get; init; }

    public string RedisAddress { get; init; }
    public string RedisPass { get; init; }

    public string DbConnectionString { get; init; }

    public string WebhookUrl { get; init; }
}
#pragma warning restore CS8618