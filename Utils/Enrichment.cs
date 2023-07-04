using System.Diagnostics;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Tack.Utils;

public class UnfragmentedHeapSizeEnricher : ILogEventEnricher
{
    private readonly SizeFormatting _sizeFormatting;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UnfragmentedHeapSize", GC.GetTotalMemory(false).FormatSize(_sizeFormatting)));
    }

    public UnfragmentedHeapSizeEnricher(SizeFormatting formatting)
    {
        _sizeFormatting = formatting;
    }
}

public class PrivateMemorySizeEnricher : ILogEventEnricher
{
    private readonly SizeFormatting _sizeFormatting;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("PrivateMemorySize", Process.GetCurrentProcess().PrivateMemorySize64.FormatSize(_sizeFormatting)));
    }

    public PrivateMemorySizeEnricher(SizeFormatting formatting)
    {
        _sizeFormatting = formatting;
    }
}

public class PagedMemorySizeEnricher : ILogEventEnricher
{
    private readonly SizeFormatting _sizeFormatting;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("PagedMemorySize", Process.GetCurrentProcess().PagedMemorySize64.FormatSize(_sizeFormatting)));
    }

    public PagedMemorySizeEnricher(SizeFormatting formatting)
    {
        _sizeFormatting = formatting;
    }
}

public class PeakPagedMemorySizeEnricher : ILogEventEnricher
{
    private readonly SizeFormatting _sizeFormatting;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("PeakPagedMemorySize", Process.GetCurrentProcess().PeakPagedMemorySize64.FormatSize(_sizeFormatting)));
    }

    public PeakPagedMemorySizeEnricher(SizeFormatting formatting)
    {
        _sizeFormatting = formatting;
    }
}

public class VirtualMemorySizeEnricher : ILogEventEnricher
{
    private readonly SizeFormatting _sizeFormatting;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("VirtualMemorySize", Process.GetCurrentProcess().VirtualMemorySize64.FormatSize(_sizeFormatting)));
    }

    public VirtualMemorySizeEnricher(SizeFormatting formatting)
    {
        _sizeFormatting = formatting;
    }
}

public class PeakVirtualMemorySizeEnricher : ILogEventEnricher
{
    private readonly SizeFormatting _sizeFormatting;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("PeakVirtualMemorySize", Process.GetCurrentProcess().PeakVirtualMemorySize64.FormatSize(_sizeFormatting)));
    }

    public PeakVirtualMemorySizeEnricher(SizeFormatting formatting)
    {
        _sizeFormatting = formatting;
    }
}

public static class EnrichmentExtensions
{
    public static LoggerConfiguration WithUnfragmentedHeapSize(this LoggerEnrichmentConfiguration cfg, SizeFormatting formatting = SizeFormatting.Bytes)
    {
        ArgumentNullException.ThrowIfNull(cfg, nameof(cfg));
        return cfg.With(new UnfragmentedHeapSizeEnricher(formatting));
    }

    public static LoggerConfiguration WithPrivateMemorySize(this LoggerEnrichmentConfiguration cfg, SizeFormatting formatting = SizeFormatting.Bytes)
    {
        ArgumentNullException.ThrowIfNull(cfg, nameof(cfg));
        return cfg.With(new PrivateMemorySizeEnricher(formatting));
    }

    public static LoggerConfiguration WithPagedMemorySize(this LoggerEnrichmentConfiguration cfg, SizeFormatting formatting = SizeFormatting.Bytes)
    {
        ArgumentNullException.ThrowIfNull(cfg, nameof(cfg));
        return cfg.With(new PagedMemorySizeEnricher(formatting));
    }

    public static LoggerConfiguration WithPeakPagedMemorySize(this LoggerEnrichmentConfiguration cfg, SizeFormatting formatting = SizeFormatting.Bytes)
    {
        ArgumentNullException.ThrowIfNull(cfg, nameof(cfg));
        return cfg.With(new PeakPagedMemorySizeEnricher(formatting));
    }

    public static LoggerConfiguration WithVirtualMemorySize(this LoggerEnrichmentConfiguration cfg, SizeFormatting formatting = SizeFormatting.Bytes)
    {
        ArgumentNullException.ThrowIfNull(cfg, nameof(cfg));
        return cfg.With(new VirtualMemorySizeEnricher(formatting));
    }

    public static LoggerConfiguration WithPeakVirtualMemorySize(this LoggerEnrichmentConfiguration cfg, SizeFormatting formatting = SizeFormatting.Bytes)
    {
        ArgumentNullException.ThrowIfNull(cfg, nameof(cfg));
        return cfg.With(new PeakVirtualMemorySizeEnricher(formatting));
    }

    public static float FormatSize(this long size,  SizeFormatting formatting)
    {
        int divisor = (int)Math.Pow(10, (int)formatting);
        return size / divisor;
    }
}

public enum SizeFormatting
{
    Bytes,
    Kilobytes,
    Megabytes,
    Gigabytes
}
