using Serilog;
using Tack.Core;
using Tack.Json;
using Tack.Utils;
using IntervalTimer = System.Timers.Timer;

namespace Tack.Handlers;
internal static class EventsHandler
{
    private static bool BaroActive { get; set; } = false;
    private static WarframeNewsObj LatestNews { get; set; } = new WarframeNewsObj();
    public static void Start()
    {
        IntervalTimer timer = new IntervalTimer();
        timer.Interval = TimeSpan.FromMinutes(2.5).TotalMilliseconds;
        timer.AutoReset = true;
        timer.Enabled = true;

        timer.Elapsed += WarframeUpdates;
        Log.Debug($"{typeof(EventsHandler)} started");
    }

    private static async void WarframeUpdates(object? sender, System.Timers.ElapsedEventArgs e)
    {
        VoidTrader? baro = ObjectCaching.GetCachedObject<VoidTrader>("baro_data")
            ?? await ExternalAPIHandler.GetBaroInfo();
        WarframeNewsObj[]? news = await ExternalAPIHandler.GetWarframeNews();

        if (baro is null || news is null) return;
        // Don't trigger anything in first 30 minutes
        if ((DateTime.Now - MainClient.StartupTime).TotalMinutes < 30)
        {
            BaroActive = baro.Active;
            LatestNews = news[0];
            Log.Debug($"Set: news = {news[0].Message}, baro = {baro.Active}");
            return;
        }

        // Baro arrive
        if (!BaroActive && baro.Active)
        {
            MessageHandler.SendMessage("pajlada",$"Void trader Baro Ki’Teer has arrived at {baro.Location}! 💠");
            MessageHandler.SendMessage(Config.RelayChannel, $"Void trader Baro Ki’Teer has arrived at {baro.Location}! 💠");
            BaroActive = true;
            int departsInSeconds = (int)(baro.Expiry.ToLocalTime() - DateTime.Now.ToLocalTime()).TotalSeconds;
            ObjectCaching.CacheObject("baro_data", baro, departsInSeconds);
        }
        // Baro Depart
        if (BaroActive && !baro.Active)
        {
            MessageHandler.SendMessage("pajlada", $"Void trader Baro Ki’Teer has departed! 💠");
            BaroActive = false;
        }

        // Skip if nothing changes
        if (LatestNews.Message == news[0].Message) return;
        // If the news is an update, send to pajlada's chat
        if (news[0].Update)
        {
            MessageHandler.SendColoredMessage(
                "pajlada",
                $"Warframe update 🚨 {news[0].Message} ( {news[0].Link} ) 🚨 ",
                ChatColor.Red);
        }
        // Send news regardless to relay channel
        MessageHandler.SendColoredMessage(
            Config.RelayChannel,
            $"Warframe news updated! {news[0].Message} ( {news[0].Link} )",
            ChatColor.CadetBlue);
        // Set new news as latest
        LatestNews = news[0];
    }
}
