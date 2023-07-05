using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog.Core;
using Serilog.Events;

namespace Bot.Utils.Logging;

public class DiscordSink : ILogEventSink
{
    private readonly JsonSerializerOptions _sOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private readonly ILogger _logger = ForContext("ShouldLogToDiscord", false).ForContext<DiscordSink>();
    private readonly ConcurrentQueue<object> _logQueue = new();
    private readonly HttpClient _client = new();
    private readonly string _webhookUrl;
    private readonly LogEventLevel _logLevel;
    private readonly LogEventLevel _propsLevel;
    private readonly Task _caller;

    public DiscordSink(string webhookUrl, LogEventLevel restrictedToMinimumLevel, LogEventLevel propsRestrictedToMinimumLevel)
    {
        _webhookUrl = webhookUrl;
        _logLevel = restrictedToMinimumLevel;
        _propsLevel = propsRestrictedToMinimumLevel;
        _caller = Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                if (_logQueue.TryDequeue(out object? logEvent) && logEvent is not null)
                {
                    HttpResponseMessage response = await _client.PostAsJsonAsync(webhookUrl, logEvent);
                    if (response.IsSuccessStatusCode)
                        _logger.Verbose("[{ClassName}] Sending log: {Status}", response.StatusCode);
                    else
                        _logger.Warning("[{ClassName}] Sending log: {Status}", response.StatusCode);
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }, TaskCreationOptions.LongRunning);
    }

    public void Emit(LogEvent logEvent)
    {
        if (!ShouldLogEvent(logEvent))
            return;


    }

    public object CreatExceptionLogObject(LogEvent log)
    {
        var (title, color) = GetEmbedData(log.Level);
        var discordMessage = new
        {
            embeds = new[]
            {
                new
                {
                    title,
                    description =  $"`{log.Exception!.GetType().Name}:` {log.RenderMessage()}",
                    color,
                    fields = new[]
                    {
                        new
                        {
                            name = "Message:",
                            value = FormatExceptionMessage(log.Exception!.Message)
                        },
                        new
                        {
                            name = "StackTrace:",
                            value = FormatExceptionMessage(log.Exception.StackTrace ?? string.Empty)
                        },
                        new
                        {
                            name = "Properties:",
                            value = FormatProperties(log)
                        }
                    }
                }
            }
        };

        return discordMessage;
    }

    public object CreateLogObject(LogEvent log)
    {
        var (title, color) = GetEmbedData(log.Level);
        var discordMessage = new
        {
            embeds = new[]
            {
                new
                {
                    title,
                    description = log.RenderMessage(),
                    color,
                    fields = log.Level < _propsLevel ? null : new[]
                    {
                        new
                        {
                            name = "Properties:",
                            value = FormatProperties(log)
                        }
                    }
                }
            }
        };

        return discordMessage;
    }

    private static string FormatExceptionMessage(string message)
    {
        if (message.Length > 900)
            message = message[..900] + " ...";
        if (!string.IsNullOrWhiteSpace(message))
            message = $"```{message}```";

        return message;
    }

    private static string FormatProperties(LogEvent logEvent)
    {
        var sb = new StringBuilder();
        _ = sb.AppendLine("```yaml");
        foreach (KeyValuePair<string, LogEventPropertyValue> entry in logEvent.Properties)
        {
            _ = sb.AppendLine($"{entry.Key}: {entry.Value}");
        }

        _ = sb.AppendLine("```");
        return sb.ToString();
    }

    private (string title, int color) GetEmbedData(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose => ("📢 Verbose", 10197915),
        LogEventLevel.Debug => ("🔍 Debug", 16777215),
        LogEventLevel.Information => ("ℹ Information", 3901635),
        LogEventLevel.Warning => ("⚠ Warning", 16312092),
        LogEventLevel.Error => ("❌ Error", 13632027),
        LogEventLevel.Fatal => ("💥 Fatal", 3866640),
        _ => default
    };

    private bool ShouldLogEvent(LogEvent logEvent)
    {
        if (logEvent.Level < _logLevel || (logEvent.Properties.ContainsKey("ShouldLogToDiscord") && !bool.Parse(logEvent.Properties["ShouldLogToDiscord"].ToString())))
            return false;

        return true;
    }
}
