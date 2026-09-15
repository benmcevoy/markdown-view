using System.CommandLine;
using http;
using ragd.Core;

namespace ragd.Cli;

public class StopCommand : Command
{
    private readonly LifeCycleManager _lifeCycleManager;
    private readonly Client _client;

    public StopCommand(LifeCycleManager lifeCycleManager, Client client) : base("stop", "Stop the RAG daemon.")
    {
        _lifeCycleManager = lifeCycleManager;
        _client = client;

        SetAction(Stop);
    }

    private async Task Stop(ParseResult parseResult)
    {
        parseResult.Log("Stopping RAG daemon...");

        if (!_lifeCycleManager.IsRunning())
        {
            parseResult.Out(ApiResponse.Stopped());

            return;
        }

        var response = await _client.SendAsync(new Request { Method = http.HttpMethod.POST, Path = "stop" });

        await WaitForShutdown(parseResult);

        parseResult.Out(response.AsApiResponse());
    }

    private async Task WaitForShutdown(ParseResult parseResult)
    {
        var timeout = DateTime.UtcNow.AddSeconds(2);
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        while (_lifeCycleManager.IsRunning() && DateTime.UtcNow < timeout)
        {
            if (!parseResult.IsQuiet()) Console.Write(".");

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        if (!parseResult.IsQuiet()) Console.WriteLine();
    }
}
