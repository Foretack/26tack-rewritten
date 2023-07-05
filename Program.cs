using Bot.Enums;
using Bot.Workflows;

namespace Bot;

public static class Program
{
    static async Task Main()
    {
        var runner = new WorkflowRunner()
           .Add<LoggerSetup>()
           .Add<ConfigSetup>()
           .Add<RedisSetup>()
           .Add<NpgsqlSetup>()
           .Add<MainClientSetup>()
           .Add<AnonClientSetup>()
           .Add<ChannelsSetup>();

        await foreach (WorkflowState result in runner.RunAll())
        {
            if (result != WorkflowState.Completed)
                throw new NotSupportedException(result.ToString());
        }

        _ = Console.ReadLine();
    }
}
