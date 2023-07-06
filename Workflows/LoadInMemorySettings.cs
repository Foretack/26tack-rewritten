using Bot.Enums;
using Bot.Interfaces;
using Bot.Models;

namespace Bot.Workflows;

internal class LoadInMemorySettings : IWorkflow
{
    public static InMemorySettings Settings { get; private set; } = default!;

    public async ValueTask<WorkflowState> Run()
    {
        try
        {
            Settings = await Cache.FetchObjectAsync("bot:settings", () => Task.FromResult(new InMemorySettings()
            {
                EnabledModules = new()
            }));
        }
        catch (Exception ex)
        {
            ForContext<LoadInMemorySettings>().Fatal(ex, "[{ClassName}] Failed to load app settings");
            return WorkflowState.Failed;
        }

        return WorkflowState.Completed;
    }
}
