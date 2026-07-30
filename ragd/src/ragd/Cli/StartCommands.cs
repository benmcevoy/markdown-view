using System.CommandLine;
using Microsoft.Extensions.Logging;
using ragd.Cli.Options;

namespace ragd.Cli;

public class StartCommand : Command
{
    private readonly LifeCycleManager _lifeCycleManager;
    private readonly ILogger _logger;

    public StartCommand(LifeCycleManager lifeCycleManager, ILogger logger) : base("start", "Start the RAG daemon.")
    {
        _lifeCycleManager = lifeCycleManager;
        _logger = logger;

        Options.Add(new DatabaseOption());
        Options.Add(new ModelOption());

        SetAction(Start);
    }

    private void Start(ParseResult parseResult)
    {
        var database = parseResult.GetRequiredValue<FileInfo>(DatabaseOption.OptionName);
        var model = parseResult.GetRequiredValue<FileInfo>(ModelOption.OptionName);

        _logger.LogInformation("Starting RAG daemon...");

        var state = _lifeCycleManager.StartDaemon(database, model);

        parseResult.Out($"Status: {state}");
    }
}
