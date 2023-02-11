global using Serilog;
using System.Diagnostics;
using System.Reflection;
using CliWrap;
using CliWrap.Buffered;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Tack.Database;
using Tack.Handlers;
using Tack.Models;
using Tack.Utils;

namespace Tack.Core;
public static class Program
{
    #region Properties
    public static ProgramSettings Settings { get; private set; } = default!;
    public static LoggingLevelSwitch LogSwitch { get; } = new LoggingLevelSwitch();
    public static DateTime StartupTime { get; private set; } = new DateTime();

    private static readonly string _assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? throw new ArgumentException($"{nameof(_assemblyName)} can not be null.");
    #endregion

    #region Main
    public static async Task Main()
    {
        LogSwitch.MinimumLevel = OperatingSystem.IsWindows() ? Serilog.Events.LogEventLevel.Verbose : Serilog.Events.LogEventLevel.Information;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(LogSwitch)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} | {Level}]{NewLine} {Message}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
            .WriteTo.Discord(AppConfigLoader.Config.LoggingWebhookUrl, restrictedToMinimumLevel: LogEventLevel.Debug)
            .CreateLogger();

        var db = new DbQueries();
        while (db.ConnectionState != System.Data.ConnectionState.Open)
        {
            Log.Warning("Bad database state: " + db.ConnectionState.ToString());
            await Task.Delay(1000);
        }

        StartupTime = DateTime.Now;

        _ = new Redis($"{AppConfigLoader.Config.RedisHost},password={AppConfigLoader.Config.RedisPass}");

        Settings = await Redis.Cache.FetchObjectAsync<ProgramSettings>("bot:settings", () =>
        Task.FromResult(new ProgramSettings() { LogLevel = LogEventLevel.Information, EnabledModules = new() }));
        LogSwitch.MinimumLevel = Settings.LogLevel;

        await MainClient.Initialize();
        await AnonymousClient.Initialize();
        MessageHandler.Initialize();
        CommandHandler.Initialize();
        ModulesHandler.Initialize();
        await DiscordClient.Initialize();

        int seconds = 0;
        while (!MainClient.Connected)
        {
            await Task.Delay(1000);
            seconds++;
            if (seconds >= 10)
                RestartProcess("startup timed out");
        }

        Log.Information("All clients are connected");
        await ChannelHandler.Connect(false);

        Time.DoEvery(TimeSpan.FromMinutes(1), Settings.UpdateCachedSettings);

        while (true)
        {
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
                continue;

            if (Enum.TryParse<LogEventLevel>(input, out LogEventLevel level))
            {
                LogSwitch.MinimumLevel = level;
                Console.WriteLine($"Switching logging level to: {level}");
            }
        }
    }
    #endregion

    #region Process methods
    // TODO: Don't rely on restarts
    public static void RestartProcess(string triggerSource)
    {
        Log.Fatal("The program is being restarted by {source} ...", triggerSource);

        _ = new DbQueries();
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
