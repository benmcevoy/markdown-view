using System.CommandLine;
using http;
using ragd.Cli.Options;

namespace ragd.Cli;

public class Commands
{
    private readonly RootCommand _rootCommand = new("CLI tool to index and query semantic embeddings.");

    public Commands(Config config, LifeCycleManager lifeCycleManager, Client client)
    {
        _rootCommand.Subcommands.Add(new IndexCommand(config, lifeCycleManager, client));
        _rootCommand.Subcommands.Add(new ListenCommand(lifeCycleManager));
        _rootCommand.Subcommands.Add(new NewCommand(config));
        _rootCommand.Subcommands.Add(new QueryCommand(config, lifeCycleManager, client));
        _rootCommand.Subcommands.Add(new StartCommand(config, lifeCycleManager));
        _rootCommand.Subcommands.Add(new StopCommand(lifeCycleManager, client));
        _rootCommand.Subcommands.Add(new StatusCommand(lifeCycleManager, client));
        _rootCommand.Subcommands.Add(new ConfigCommand(config, lifeCycleManager, client));

        // add global options
        foreach (var cmd in _rootCommand.Subcommands)
        {
            cmd.Options.Add(new QuietOption());
            cmd.Options.Add(new JsonOption());
        }
    }

    public ParseResult Parse(string[] args) => _rootCommand.Parse(args);
}