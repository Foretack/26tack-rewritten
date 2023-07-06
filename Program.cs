﻿using Bot.Enums;
using Bot.Workflows;
using Serilog.Events;

namespace Bot;

public static class Program
{
    static async Task Main()
    {
        var runner = new WorkflowRunner()
           .Add<ConfigSetup>()
           .Add<LoggerSetup>()
           .Add<RedisSetup>()
           .Add<NpgsqlSetup>()
           .Add<LoadInMemorySettings>()
           .Add<MainClientSetup>()
           .Add<AnonClientSetup>()
           .Add<ChannelsSetup>()
           .Add<LoadWhiteListBlackList>()
           .Add<InitHandlers>();

        await foreach (WorkflowState result in runner.RunAll())
        {
            if (result != WorkflowState.Completed)
                throw new NotSupportedException(result.ToString());
        }

        while (true)
        {
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
                continue;

            if (Enum.TryParse(input, true, out LogEventLevel level))
            {
                LoggerSetup.LogSwitch.MinimumLevel = level;
                Console.WriteLine($"Switching logging level to: {level}");
            }
        }
    }
}
