using System.CommandLine;
using http;
using ragd.Core;

namespace ragd.Cli;

public class StatusCommand : Command
{
    private readonly LifeCycleManager _lifeCycleManager;
    private readonly Client _client;

    public StatusCommand(LifeCycleManager lifeCycleManager, Client client) : base("status", "Get status of daemon.")
    {
        _lifeCycleManager = lifeCycleManager;
        _client = client;
        SetAction(Status);
    }

    private async Task Status(ParseResult parseResult)
    {
        parseResult.Log("Checking RAG daemon status...");

        if (!_lifeCycleManager.IsRunning())
        {
            parseResult.Out(ApiResponse.Stopped());

            return;
        }

        var response = await _client.SendAsync(new Request { Method = http.HttpMethod.GET, Path = "status" });

        parseResult.Out(response.AsApiResponse());
    }
}
