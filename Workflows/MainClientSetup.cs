﻿global using static Bot.Workflows.MainClientSetup;
using Bot.Enums;
using Bot.Interfaces;
using Microsoft.Extensions.Logging;
using MiniTwitch.Irc;

namespace Bot.Workflows;

internal class MainClientSetup : IWorkflow
{
    public static IrcClient MainClient { get; private set; } = default!;

    public async ValueTask<WorkflowState> Run()
    {
        MainClient = new(options =>
        {
            options.Username = Config.Username;
            options.OAuth = Config.Token;
            options.Logger = new LoggerFactory().AddSerilog(ForContext("IsSubLogger", true).ForContext("Client", "Main")).CreateLogger<IrcClient>();
        });

        bool connected = await MainClient.ConnectAsync();
        if (!connected)
        {
            ForContext<MainClientSetup>().Fatal("[{ClassName}] Failed to setup MainClient");
            return WorkflowState.Failed;
        }

        Information("MainClient setup done");
        return WorkflowState.Completed;
    }
}