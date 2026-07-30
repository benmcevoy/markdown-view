using System.CommandLine;
using Microsoft.Extensions.Logging;
using ragd.Cli.Options;

namespace ragd.Cli;

public class ListenCommand : Command
{
    private readonly LifeCycleManager _lifeCycleManager;
    private readonly ILogger _logger;

    public ListenCommand(LifeCycleManager lifeCycleManager, ILogger logger) : base("listen")
    {
        _lifeCycleManager = lifeCycleManager;
        _logger = logger;

        Hidden = true;

        Options.Add(new DatabaseOption());
        Options.Add(new ModelOption());

        SetAction(Listen);
    }

    private void Listen(ParseResult parseResult)
    {
        var database = parseResult.GetRequiredValue<FileInfo>(DatabaseOption.OptionName);
        var model = parseResult.GetRequiredValue<FileInfo>(ModelOption.OptionName);

        _lifeCycleManager.Listen(database, model);
    }
}
