using Dasync.Collections;
using Serilog;
using Tack.Json;
using Tack.Utils;
using C = Tack.Core.Core;
using Db = Tack.Database.Database;
using IntervalTimer = System.Timers.Timer;

namespace Tack.Handlers;
internal static class EventsHandler
{
    #region Properties
    private static AsyncEnumerable<Event>? Events { get; set; }
    private static bool BaroActive { get; set; } = false;
    private static WarframeNewsObj LatestNews { get; set; } = new WarframeNewsObj();
    private static readonly Random R = new Random();
    #endregion

    #region Initialization
    public static async Task Start()
    {
        IntervalTimer timer = new IntervalTimer();
        timer.Interval = TimeSpan.FromMinutes(2.5).TotalMilliseconds;
        timer.AutoReset = true;
        timer.Enabled = true;
        timer.Elapsed += WarframeUpdates;

        await LoadEvents();

        Log.Debug($"{typeof(EventsHandler)} started");
    }

    private static async Task LoadEvents()
    {
        Db db = new Db();
        var e = await db.LoadEvents();
        if (e is null)
        {
            Events = null;
            Log.Error("Loading events returned null");
            return;
        }
        Events = e;
    }
    #endregion

    #region Handling
    public static async ValueTask CheckTrigger(Trigger trigger)
    {
        if (Events is null || await Events.CountAsync() == 0) return;
        if (trigger.Type is not null)
        {
            var matching = await Task.Run(() => Events
            .WhereAwait(async x => await Task.Run(() => x.Type == trigger.Type))
            .SelectAwait(async y => await Task.Run(() => y)));

            await matching.ForEachAsync(async m => await FireEvent(m, trigger.Content ?? string.Empty));
            return;
        }

        await Events.ForEachAsync(async e =>
        {
            // Split multiple identifiers
            string[] siArgs = e.Identifier.Split(",,");
            // Identifier count
            int iLength = siArgs.Length;
            // Amount of identifiers matched
            int matchCount = 0;

            foreach (string siArg in siArgs)
            {
                string[] iArgs = siArg.Split(':');

                if (iArgs[0] == "source" && iArgs[1] == "contains")
                {
                    if (trigger.Source.Contains(iArgs[2])) matchCount++;
                    continue;
                }
                if (iArgs[0] == "source" && iArgs[1] == "equals")
                {
                    if (trigger.Source.Equals(iArgs[2])) matchCount++;
                    continue;
                }
                if (iArgs[0] == "content" && iArgs[1] == "contains")
                {
                    if (trigger.Content is not null && trigger.Content.Contains(iArgs[2])) matchCount++;
                    continue;
                }
                if (iArgs[0] == "content"
                && iArgs[1] == "equals"
                && trigger.Content is not null
                && trigger.Content.Equals(iArgs[2])) matchCount++;
            }

            // The event is fired only when the matched amount is equal to the identifier count
            // i.e: when each identifier is matched
            if (matchCount == iLength)
            {
                Event triggeredEvent = new Event(
                    e.Type,
                    e.Identifier,
                    trigger.Source,
                    trigger.Source.ShortenSource(),
                    e.Destination,
                    e.Formatting,
                    e.Args);
                await FireEvent(triggeredEvent, trigger.Content ?? string.Empty);
                Log.Verbose($"Event fired {triggeredEvent}");
            } 
        });
    }

    private static async ValueTask FireEvent(Event e, string tContent)
    {
        string[] dArgs = e.Destination.Split(':');
        if (dArgs[0] == "twitch")
        {
            string channel = dArgs[1];
            string? message = tContent.WithFormatOf(e).WithArgsOf(e);
            if (message is null) return;
            MessageHandler.SendMessage(channel, message);
        }
        if (dArgs[0] == "discord")
        {
            ulong guild = ulong.Parse(dArgs[1]);
            ulong channel = ulong.Parse(dArgs[2]);
            string? message = tContent.WithFormatOf(e).WithArgsOf(e);
            if (message is null) return;
            await MessageHandler.SendDiscordMessage(guild, channel, message);
        }
        if (dArgs[0] == "db")
        {
            //
        }
    }

    /// <returns> Content string formatted with the specified event's formatting settings </returns>
    private static string WithFormatOf(this string str, Event e)
    {
        if (e.Formatting is null) return str;
        
        string text = Options.ParseString("TEXT", str, "->", '|')
                ?? "<notext>";
        int embedcount = Options.ParseInt("EMBEDCOUNT", str, "->", '|')
            ?? 0;
        string embedtitle = Options.ParseString("EMBEDTITLE", str, "->", '|')
            ?? "<notitle>";
        string embeddesc = Options.ParseString("EMBEDDESC", str, "->", '|')
            ?? "<nodesc>";
        string embedlink = Options.ParseString("EMBEDLINK", str, "->", '|')
            ?? "<nolink>";
        string from = Options.ParseString("FROM", str, "->", '|')
            ?? "<nofrom>";

        return e.Formatting
            .Replace("TEXT", text)
            .Replace("EMBEDCOUNT", embedcount.ToString())
            .Replace("EMBEDTITLE", embedtitle)
            .Replace("EMBEDDESC", embeddesc)
            .Replace("EMBEDLINK", embedlink)
            .Replace("FROM", from);
    }
    private static string? WithArgsOf(this string str, Event e)
    {
        if (e.Args is null) return str;
        string args = e.Args;

        int chance = Options.ParseInt("chance", args, splitter: ';')
            ?? 100;
        bool allowOnline = Options.ParseBool("allow_online", args, splitter: ';')
            ?? true;
        int charlimit = Options.ParseInt("char_limit", args, splitter: ';')
            ?? 500;

        int roll = R.Next(100);
        if (roll > chance) return null;

        string? d = e.Destination.StartsWith("twitch:") 
            ? e.Destination.Split(':')[1]
            : null;
        if (!allowOnline && d is not null && StreamMonitor.StreamData[d].IsOnline) return null;

        if (str.Length > charlimit) return str[..charlimit];
        
        return str;
    }
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

// TEXT->|EMBEDCOUNT->|EMBEDTITLE->|EMBEDDESC->|EMBEDLINK->|FROM->|

/// <param name="Type"> The type of this event </param>
/// <param name="Identifier"> 
/// How this event is identified
/// <list type="bullet|number|table">
///    <item>
///        <term>source:contains:value</term>
///        <description>Source of the event contains value</description>
///    </item>
///    <item>
///        <term>source:equals:value</term>
///        <description>Source of the event is equal to value</description>
///    </item>
///    <item>
///        <term>content:contains:value</term>
///        <description>Content of the event contains value</description>
///    </item>
///    <item>
///        <term>content:equals:value</term>
///        <description>Content of the event is equal to value</description>
///    </item>
///</list>
/// </param>
/// <param name="Source"> Where the event comes from </param>
/// <param name="SourceShort"> Source with nicer formatting </param>
/// <param name="Destination">
/// Where to relay the event
/// <list type="bullet|number|table">
///    <item>
///        <term>twitch:channel</term>
///        <description> Specified channel's chat </description>
///    </item>
///    <item>
///        <term>discord:guildid:channelid</term>
///        <description> Specified Discord channel </description>
///    </item>
///    <item>
///        <term>db:table</term>
///        <description> Specified database table </description>
///    </item>
///</list>
/// </param>
/// <param name="Formatting"> How the event text will be relayed </param>
/// <param name="Args">
/// Additional options 
/// <list type="bullet|number|table">
///    <item>
///        <term>chance:number</term>
///        <description> Only relay event after rolling this chance </description>
///    </item>
///    <item>
///        <term>allow_online:bool</term>
///        <description> Whether the event can be sent in an online stream or not </description>
///    </item>
///    <item>
///        <term>char_limit:number</term>
///        <description> Cut the message and add '...' after the specified amount of characters </description>
///    </item>
///</list>
/// </param>
public record Event(string Type, string Identifier, string Source, string? SourceShort, string Destination, string? Formatting, string? Args);
public record struct Trigger(string? Type, string Source, string? Content);
