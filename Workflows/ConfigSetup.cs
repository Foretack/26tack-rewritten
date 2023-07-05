global using static Bot.Workflows.ConfigSetup;
using Bot.Enums;
using Bot.Interfaces;
using Bot.Models;
using Microsoft.Extensions.Configuration;

namespace Bot.Workflows;

public class ConfigSetup : IWorkflow
{
    public static AppConfig Config { get; private set; } = default!;

    public ValueTask<WorkflowState> Run()
    {
        var builder = new ConfigurationBuilder();
        IConfigurationRoot config = builder.AddJsonFile("BotConfig.json").Build();
        var section = new AppConfig();
        string mode = OperatingSystem.IsLinux() ? "Hosted" : "Local";
        config.Bind(mode, section);
        Config = section;
        Information("Loaded {Mode} config", mode);

        return ValueTask.FromResult(WorkflowState.Completed);
    }
}
