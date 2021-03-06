using System.Diagnostics;
using System.Reflection;
using Serilog;
using Serilog.Core;
using Tack.Handlers;
using CliWrap;
using CliWrap.Buffered;
using Tack.Database;

namespace Tack.Core;
public static class Core
{
    #region Properties
    public static LoggingLevelSwitch LogSwitch { get; } = new LoggingLevelSwitch();
    public static DateTime StartupTime { get; private set; } = new DateTime();

    private static string AssemblyName { get; } = Assembly.GetEntryAssembly()?.GetName().Name ?? throw new ArgumentException($"{nameof(AssemblyName)} can not be null.");
    #endregion

    #region Main
    public static async Task<int> Main()
    {
        LogSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Information;
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(LogSwitch)
            .WriteTo.Console(outputTemplate: "<<{Timestamp:HH:mm:ss} <~> {Level}>>{NewLine} {Message}{NewLine}{Exception}{NewLine}")
            .CreateLogger();

        DbQueries db = new DbQueries();
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

        int seconds = 0;
        while (!(AnonymousClient.Connected 
        && DiscordClient.Connected 
        && MainClient.Connected))
        {
            await Task.Delay(1000);
            seconds++;
            if (seconds >= 10) RestartProcess("startup timed out");
        }
        Log.Information("All clients are connected");
        await ChannelHandler.Connect(false);

        Console.ReadLine();
        return 0;
    }
    #endregion

    #region Process methods
    public static void RestartProcess(string triggerSource)
    {
        Log.Fatal($"The program is restarting...");
        DbQueries db = new DbQueries();
        #pragma warning disable CS4014
        db.LogException(new ApplicationException($"PROGRAM RESTARTED BY {triggerSource}"));
        #pragma warning restore CS4014
        Process.Start($"./{AssemblyName}", Environment.GetCommandLineArgs());
        Environment.Exit(0);
    }
    #endregion

    public static async Task<string?> GitPull()
    {
        try
        {
            var pullResults = await Cli.Wrap("git").WithArguments("pull").ExecuteBufferedAsync();
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
