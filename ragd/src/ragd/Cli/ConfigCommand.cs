using System.CommandLine;
using System.Text.Json;
using http;
using ragd.Core;

namespace ragd.Cli;

public class ConfigCommand : Command
{
    private readonly Config _config;
    private readonly LifeCycleManager _lifeCycleManager;
    private readonly Client _client;

    public ConfigCommand(Config config, LifeCycleManager lifeCycleManager, Client client) : base("config", "View config of daemon.")
    {

        SetAction(Config);
        _config = config;
        _lifeCycleManager = lifeCycleManager;
        _client = client;
    }

    private async Task Config(ParseResult parseResult)
    {
        parseResult.Log("Checking RAG daemon config...");

        if (!_lifeCycleManager.IsRunning())
        {
            parseResult.Out(FormatResponse(ApiResponse.Stopped(_config)));

            return;
        }

        var response = await _client.SendAsync(new Request { Method = http.HttpMethod.GET, Path = "config" });

        parseResult.Out(FormatResponse(response.AsApiResponse()));
    }

    private string FormatResponse(ApiResponse response) =>
         JsonSerializer.Serialize(response.Data, new JsonSerializerOptions(JsonSerializerOptions.Web) { WriteIndented = true });
}
