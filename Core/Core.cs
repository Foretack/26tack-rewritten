using Serilog;
using Serilog.Core;
using Tack.Handlers;
using Db = Tack.Database.Database;

namespace Tack.Core;
public static class Core
{
    public static LoggingLevelSwitch LogSwitch { get; } = new LoggingLevelSwitch();
    public static DateTime StartupTime { get; private set; } = new DateTime();

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
}
