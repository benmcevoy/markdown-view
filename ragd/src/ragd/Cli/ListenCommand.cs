using System.CommandLine;
using ragd.Cli.Options;

namespace ragd.Cli;

public class ListenCommand : Command
{
    private readonly LifeCycleManager _lifeCycleManager;

    public ListenCommand(LifeCycleManager lifeCycleManager) : base("listen")
    {
        _lifeCycleManager = lifeCycleManager;

        Hidden = true;

        Options.Add(new DatabaseOption(optional: true));
        Options.Add(new ModelOption(optional: true));

        SetAction(Listen);
    }

    private void Listen(ParseResult parseResult)
    {
        var database = parseResult.GetValue<FileInfo>(DatabaseOption.OptionName);
        var model = parseResult.GetValue<FileInfo>(ModelOption.OptionName);

        _lifeCycleManager.Listen(database, model);
    }
}
