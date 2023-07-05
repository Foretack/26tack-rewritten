using Serilog.Core;
using Serilog.Events;

namespace Bot.Utils.Logging;
public class HeapSizeEnricher : ILogEventEnricher
{
    private readonly LogEventLevel _maxEnrichmentLevel;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Level > _maxEnrichmentLevel)
            return;

        string memory = $"{GC.GetTotalMemory(false) / 1000000} MB";
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("HeapSize", memory));
    }

    public HeapSizeEnricher(LogEventLevel maxEnrichmentLevel)
    {
        _maxEnrichmentLevel = maxEnrichmentLevel;
    }
}
