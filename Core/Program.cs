global using Serilog;
global using static Tack.AppConfigLoader;
using System.Reflection;
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
    #endregion

    #region Main
    public static async Task Main()
    {
        LogSwitch.MinimumLevel = LogEventLevel.Information;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(LogSwitch)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} | {Level}]{NewLine} {Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
            .WriteTo.Discord(AppConfig.LoggingWebhookUrl, restrictedToMinimumLevel: LogEventLevel.Debug)
            .CreateLogger();

        SingleOf.Set<DbQueries>(new());
        DbQueries db = SingleOf<DbQueries>.Obj;
        while (db.ConnectionState != System.Data.ConnectionState.Open)
        {
            Log.Warning("Bad database state: " + db.ConnectionState.ToString());
            await Task.Delay(1000);
        }

        StartupTime = DateTime.Now;

        Redis.Init($"{AppConfig.RedisHost},password={AppConfig.RedisPass}");

        Settings = await Redis.Cache.FetchObjectAsync("bot:settings", () =>
        Task.FromResult(new ProgramSettings() { LogLevel = LogEventLevel.Information, EnabledModules = new() }));
        LogSwitch.MinimumLevel = Settings.LogLevel;

        SingleOf.Set<MainClient>(new());
        SingleOf.Set<AnonymousClient>(new());
        AnonymousClient anonClient = SingleOf<AnonymousClient>.Obj;
        MainClient mainClient = SingleOf<MainClient>.Obj;
        if (await anonClient.Client.ConnectAsync())
            Log.Information("[{h}] Anonymous client connected", nameof(Program));

        if (await mainClient.Client.ConnectAsync())
            Log.Information("[{h}] Main client connected", nameof(Program));

        await mainClient.SetSelf();

        SingleOf.Set<MessageHandler>(new());
        SingleOf.Set<CommandHandler>(new());
        SingleOf.Set<ModulesHandler>(new());
        await DiscordClient.Initialize();

        Log.Information("All clients are connected");
        await ChannelHandler.Connect(false);

        Time.DoEvery(TimeSpan.FromMinutes(1), Settings.UpdateCachedSettings);

        while (true)
        {
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
                continue;

            if (Enum.TryParse(input, out LogEventLevel level))
            {
                LogSwitch.MinimumLevel = level;
                Console.WriteLine($"Switching logging level to: {level}");
            }
        }
    }
    #endregion
}
