﻿using Serilog;
using Tack.Json;
using Tack.Utils;
using C = Tack.Core.Core;
using IntervalTimer = System.Timers.Timer;

namespace Tack.Handlers;
internal static class EventsHandler
{
    #region Properties
    private static bool BaroActive { get; set; } = false;
    private static WarframeNewsObj LatestNews { get; set; } = new WarframeNewsObj();
    #endregion

    #region Initialization
    public static void Start()
    {
        IntervalTimer timer = new IntervalTimer();
        timer.Interval = TimeSpan.FromMinutes(2.5).TotalMilliseconds;
        timer.AutoReset = true;
        timer.Enabled = true;
        timer.Elapsed += WarframeUpdates;

        Log.Debug($"{typeof(EventsHandler)} started");
    }
    #endregion

    #region Handling
    //
    #endregion

    #region Warframe stuff
    private static async void WarframeUpdates(object? sender, System.Timers.ElapsedEventArgs e)
    {
        VoidTrader? baro = ObjectCache.Get<VoidTrader>("baro_data")
            ?? await ExternalAPIHandler.GetBaroInfo();
        WarframeNewsObj[]? news = await ExternalAPIHandler.GetWarframeNews();

        if (baro is null || news is null) return;
        // Don't trigger anything in first 10 minutes
        if ((DateTime.Now - C.StartupTime).TotalMinutes < 10)
        {
            BaroActive = baro.Active;
            LatestNews = news[0];
            Log.Debug($"Set: news = {news[0].Message}, baro = {baro.Active}");
            return;
        }

        // Baro arrive
        if (!BaroActive && baro.Active)
        {
            MessageHandler.SendMessage("pajlada", $"Void trader Baro Ki’Teer has arrived at {baro.Location}! 💠");
            MessageHandler.SendMessage(Config.RelayChannel, $"Void trader Baro Ki’Teer has arrived at {baro.Location}! 💠");
            BaroActive = true;
            int departsInSeconds = (int)(baro.Expiry.ToLocalTime() - DateTime.Now.ToLocalTime()).TotalSeconds;
            ObjectCache.Put("baro_data", baro, departsInSeconds);
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
    #endregion
}
