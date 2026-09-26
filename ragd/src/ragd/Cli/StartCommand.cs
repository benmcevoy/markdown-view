using System.CommandLine;
using ragd.Cli.Options;
using ragd.Core;

namespace ragd.Cli;

public class StartCommand : Command
{
    private readonly Config _config;
    private readonly LifeCycleManager _lifeCycleManager;

    public StartCommand(Config config, LifeCycleManager lifeCycleManager) : base("start", "Start the RAG daemon.")
    {
        _config = config;

        _lifeCycleManager = lifeCycleManager;

        Options.Add(new DatabaseOption());
        Options.Add(new ModelOption());

        SetAction(Start);
    }

    private async Task Start(ParseResult parseResult)
    {
        parseResult.Log("Starting RAG daemon...");

        var state = _lifeCycleManager.StartDaemon(_config);

        await WaitForStart(parseResult);

        parseResult.Out(Status(state));
    }

    private static ApiResponse Status(string status) =>
         new() { Message = status, Status = "OK" };

    private async Task WaitForStart(ParseResult parseResult)
    {
        var timeout = DateTime.UtcNow.AddSeconds(2);
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        while (!_lifeCycleManager.IsRunning() && DateTime.UtcNow < timeout)
        {
            if (!parseResult.IsQuiet()) Console.Write(".");

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        if (!parseResult.IsQuiet()) Console.WriteLine();
    }
}