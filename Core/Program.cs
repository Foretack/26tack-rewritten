﻿global using Serilog;
using System.Diagnostics;
using System.Reflection;
using CliWrap;
using CliWrap.Buffered;
using Serilog.Core;
using Serilog.Events;
using Tack.Database;
using Tack.Handlers;
using Tack.Utils;

namespace Tack.Core;
public static class Program
{
    #region Properties
    public static LoggingLevelSwitch LogSwitch { get; } = new LoggingLevelSwitch();
    public static DateTime StartupTime { get; private set; } = new DateTime();

    private static string _assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? throw new ArgumentException($"{nameof(_assemblyName)} can not be null.");
    #endregion

    #region Main
    public static async Task<int> Main()
    {
        LogSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Information;
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(LogSwitch)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} | {Level}]{NewLine} {Message}{NewLine}{Exception}{NewLine}")
            .WriteTo.Discord(AppConfigLoader.Config.LoggingWebhookUrl, restrictedToMinimumLevel: LogEventLevel.Debug)
            .CreateLogger();

        var db = new DbQueries(0);

        StartupTime = DateTime.Now;

        MainClient.Initialize();
        AnonymousClient.Initialize();
        MessageHandler.Initialize();
        CommandHandler.Initialize();
        ModulesHandler.Initialize();
        DiscordClient.Initialize();

        int seconds = 0;
        while (!MainClient.Connected)
        {
            await Task.Delay(1000);
            seconds++;
            if (seconds >= 10) RestartProcess("startup timed out");
        }
        Log.Information("All clients are connected");
        await ChannelHandler.Connect(false);

        _ = Console.ReadLine();
        return 0;
    }
    #endregion

    #region Process methods
    // TODO: Don't rely on restarts
    public static void RestartProcess(string triggerSource)
    {
        Log.Fatal($"The program is restarting...");
        var db = new DbQueries();
        _ = db.LogException(new ApplicationException($"PROGRAM RESTARTED BY {triggerSource}"));
        _ = Process.Start($"./{_assemblyName}", Environment.GetCommandLineArgs());
        Environment.Exit(0);
    }
    #endregion

    public static async Task<string?> GitPull()
    {
        try
        {
            BufferedCommandResult pullResults = await Cli.Wrap("git").WithArguments("pull").ExecuteBufferedAsync();
            return pullResults.StandardOutput.Split('\n')
                .First(x => x.Contains("files changed") || x.Contains("file changed") || x.Contains("Already up to date"));
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"git pull command failed");
            return null;
        }
    }
}