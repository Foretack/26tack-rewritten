global using static Serilog.Log;
global using Serilog;
using MiniTwitch.Irc;
using Serilog.Formatting.Compact;
using Bot.Interfaces;
using Bot.Enums;
using Serilog.Enrichers.ClassName;

namespace Bot.Workflows;

public class LoggerSetup : IWorkflow
{
    public ValueTask<WorkflowState> Run()
    {
        Logger = new LoggerConfiguration()
            .Enrich.WithProperty("MiniTwitch.Irc", typeof(IrcClient).Assembly.GetName().Version)
            .Enrich.WithClassName()
            .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy.mm.dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}")
            .WriteTo.File(new CompactJsonFormatter(), "data.log", flushToDiskInterval: TimeSpan.FromMinutes(2.5), rollingInterval: RollingInterval.Month)
            .WriteTo.File("readable_data.log", flushToDiskInterval: TimeSpan.FromMinutes(2.5), rollingInterval: RollingInterval.Month)
            .CreateLogger();

        return ValueTask.FromResult(WorkflowState.Completed);
    }
}

