﻿using System.Diagnostics;
using System.Reflection;
using Serilog;
using Serilog.Core;
using Tack.Handlers;
using Db = Tack.Database.Database;

namespace Tack.Core;
public static class Core
{
    public static LoggingLevelSwitch LogSwitch { get; } = new LoggingLevelSwitch();
    public static DateTime StartupTime { get; private set; } = new DateTime();
    public static string AssemblyName { get; } = Assembly.GetEntryAssembly()?.GetName().Name ?? throw new ArgumentException($"{nameof(AssemblyName)} can not be null.");

    public static async Task<int> Main()
    {
        LogSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Information;
        Log.Logger = new LoggerConfiguration().MinimumLevel.ControlledBy(LogSwitch).WriteTo.Console().CreateLogger();

        Db db = new Db();
        Config.Auth = await db.GetAuthorizationData();
        Config.Discord = await db.GetDiscordData();
        Config.Links = new Links();

        StartupTime = DateTime.Now;

        MainClient.Initialize();
        AnonymousClient.Initialize();
        MessageHandler.Initialize();
        CommandHandler.Initialize();
        EventsHandler.Start();
        await DiscordClient.Connect();

        while (!(AnonymousClient.Connected 
        && DiscordClient.Connected 
        && MainClient.Connected))
        {
            await Task.Delay(1000);
        }
        Log.Information("All clients are connected");
        await ChannelHandler.Connect(false);

        Console.ReadLine();
        return 0;
    }

    public static void RestartProcess()
    {
        Log.Fatal($"The program is restarting...");
        Db db = new Db();
        db.LogException(new ApplicationException("PROGRAM RESTARTED")).RunSynchronously();
        Process.Start($"./{AssemblyName}", Environment.GetCommandLineArgs());
        Environment.Exit(0);
    }

    public static float GetMemoryUsage()
    {
        return (float)Math.Truncate(Process.GetCurrentProcess().PrivateMemorySize64 / Math.Pow(10, 6) * 100) / 100;
    }
}
