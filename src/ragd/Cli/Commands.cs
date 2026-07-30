using System.CommandLine;
using ragd.Cli.Options;
using ragd.Http;

namespace ragd.Cli;

public class Commands
{
    private readonly RootCommand _rootCommand = new("CLI tool to index and query semantic embeddings.");
    private readonly RagLogger _logger;

    public Commands(LifeCycleManager lifeCycleManager, Client client, RagLogger logger)
    {
        _logger = logger;

        _rootCommand.Subcommands.Add(new IndexCommand(lifeCycleManager, client, _logger));
        _rootCommand.Subcommands.Add(new ListenCommand(lifeCycleManager, _logger));
        _rootCommand.Subcommands.Add(new NewCommand(_logger));
        _rootCommand.Subcommands.Add(new QueryCommand(lifeCycleManager, client, _logger));
        _rootCommand.Subcommands.Add(new StartCommand(lifeCycleManager, _logger));
        _rootCommand.Subcommands.Add(new StopCommand(client, _logger));
        _rootCommand.Subcommands.Add(new StatusCommand(lifeCycleManager, client, _logger));

        // add global options
        foreach(var cmd in _rootCommand.Subcommands)
        {
            cmd.Options.Add(new QuietOption());
            cmd.Options.Add(new JsonOption());
        }
    }

    public ParseResult Parse(string[] args)
    {
        var parseResult = _rootCommand.Parse(args);
        var isQuiet = parseResult.IsQuiet();
        var isJson = parseResult.IsJson();

        // suppress console output for JSON or quiet
        _logger.IsQuiet = isQuiet || isJson;

        return parseResult;
    }
}