using System.CommandLine;
using Microsoft.Extensions.Logging;
using ragd.Http;

namespace ragd.Cli;

public class StopCommand : Command
{
    private readonly LifeCycleManager _lifeCycleManager;
    private readonly Client _client;
    private readonly ILogger _logger;

    public StopCommand(LifeCycleManager lifeCycleManager, Client client, ILogger logger) : base("stop", "Stop the RAG daemon.")
    {
        _lifeCycleManager = lifeCycleManager;
        _client = client;
        _logger = logger;

        SetAction(Stop);
    }

    private void Stop(ParseResult parseResult)
    {
        _logger.LogInformation("Stopping RAG daemon...");

        if (!_lifeCycleManager.IsRunning())
        {
            parseResult.Out(Stopped());

            return;
        }

        var response = _client.Send(new Request { Method = Http.HttpMethod.POST, Path = "stop" });

        parseResult.Out(response);
    }

    private static JsonResponse Stopped() => new(HttpStatusCode.ServerError) { Status = LifeCycleStates.STOPPED };
}
