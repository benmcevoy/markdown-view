using System.CommandLine;
using Microsoft.Extensions.Logging;
using ragd.Http;

namespace ragd.Cli;

public class StatusCommand : Command
{
    private readonly LifeCycleManager _lifeCycleManager;
    private readonly Client _client;
    private readonly ILogger _logger;

    public StatusCommand(LifeCycleManager lifeCycleManager, Client client, ILogger logger) : base("status", "Get status of daemon.")
    {
        _lifeCycleManager = lifeCycleManager;
        _client = client;
        _logger = logger;
        SetAction(Status);
    }

    private void Status(ParseResult parseResult)
    {
        _logger.LogInformation("Checking RAG daemon status...");

        if (!_lifeCycleManager.IsRunning())
        {
            parseResult.Out($"Status: {LifeCycleStates.STOPPED}");
            return;
        }

        var response = _client.Send(new Request { Method = Http.HttpMethod.GET, Path = "status" });

        parseResult.Out(response);
    }
}
