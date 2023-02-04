using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Tack.Utils;
internal sealed class DiscordSink : ILogEventSink
{
    private readonly ConcurrentQueue<StringContent> _logQueue = new();
    private readonly IFormatProvider _formatProvider;
    private readonly string _webhookUrl;
    private readonly LogEventLevel _restrictedToMinimumLevel;
    private readonly HttpClient _httpClient = new();

    private string _title;
    private int _color;

    public DiscordSink(IFormatProvider formatProvider, string webhookUrl, LogEventLevel restrictedToMinimumLevel)
    {
        _formatProvider = formatProvider;
        _webhookUrl = webhookUrl;
        _restrictedToMinimumLevel = restrictedToMinimumLevel;
        Time.DoEvery(TimeSpan.FromSeconds(3), SendWebhook);
    }

    public void Emit(LogEvent logEvent)
    {
        SendMessage(logEvent);
    }

    private void SendMessage(LogEvent logEvent)
    {
        if (!ShouldlogMessage(_restrictedToMinimumLevel, logEvent.Level)) return;

        SpecifyEmbedLevel(logEvent.Level);
        if (logEvent.Exception is not null)
        {
            var discordMessage = new
            {
                embeds = new[]
                {
                    new
                    {
                        title = _title,
                        description =  $"`{logEvent.Exception.GetType().Name}:` "
                            + logEvent.RenderMessage(_formatProvider),
                        color = _color,
                        fields = new[]
                        {
                            new
                            {
                                name = "Message:",
                                value = FormatMessage(logEvent.Exception.Message, 1000)
                            },
                            new
                            {
                                name = "StackTrace:",
                                value = FormatMessage(logEvent.Exception.StackTrace ?? String.Empty, 1000)
                            }
                        }
                    }
                }
            };
            StringContent content = new(JsonSerializer.Serialize(discordMessage), Encoding.UTF8, "application/json");
            _logQueue.Enqueue(content);
            return;
        }

        var discordMessage_ = new
        {
            embeds = new[]
            {
                new
                {
                    title = _title,
                    description = logEvent.RenderMessage(_formatProvider),
                    color = _color
                }
            }
        };
        StringContent content_ = new(JsonSerializer.Serialize(discordMessage_), Encoding.UTF8, "application/json");
        _logQueue.Enqueue(content_);
    }

    private void SpecifyEmbedLevel(LogEventLevel level)
    {
        switch (level)
        {
            case LogEventLevel.Verbose:
                _title = "📢 Verbose";
                _color = 10197915;
                break;
            case LogEventLevel.Debug:
                _title = "🔍 Debug";
                _color = 16777215;
                break;
            case LogEventLevel.Information:
                _title = "ℹ Information";
                _color = 3901635;
                break;
            case LogEventLevel.Warning:
                _title = "⚠ Warning";
                _color = 16312092;
                break;
            case LogEventLevel.Error:
                _title = "❌ Error";
                _color = 13632027;
                break;
            case LogEventLevel.Fatal:
                _title = "💥 Fatal";
                _color = 3866640;
                break;
            default:
                break;
        }
    }

    private async Task SendWebhook()
    {
        if (_logQueue.TryDequeue(out var content))
        {
            await _httpClient.PostAsync(_webhookUrl, content);
        }
    }

    private static string FormatMessage(string message, int maxLength)
    {
        if (message.Length > maxLength)
            message = message[..maxLength] + " ...";
        if (!string.IsNullOrWhiteSpace(message))
            message = $"```{message}```";

        return message;
    }

    private static bool ShouldlogMessage(LogEventLevel minimumLogEventLevel, LogEventLevel messageLogEventLevel) => messageLogEventLevel >= minimumLogEventLevel;
}

internal static class DiscordSinkExtensions
{
    public static LoggerConfiguration Discord(this LoggerSinkConfiguration loggerConfiguration, string webhookUrl,
                                              IFormatProvider formatProvider = default!,
                                              LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose)
    {
        return loggerConfiguration.Sink(new DiscordSink(formatProvider, webhookUrl, restrictedToMinimumLevel));
    }
}
