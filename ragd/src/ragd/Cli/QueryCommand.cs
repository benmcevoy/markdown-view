using System.Collections.ObjectModel;
using System.CommandLine;
using ragd.Cli.Options;
using http;
using ragd.Core;

namespace ragd.Cli;

public class QueryCommand : Command
{
    private readonly Config _config;
    private readonly LifeCycleManager _lifeCycleManager;
    private readonly Client _client;
    private const string ArgumentName = "query";


    public QueryCommand(Config config, LifeCycleManager lifeCycleManager, Client client)
        : base("query", "Query the index for semantically matching results.")
    {
        _config = config;
        _lifeCycleManager = lifeCycleManager;
        _client = client;

        Options.Add(new NameOption());
        Options.Add(new LimitOption());
        Arguments.Add(new Argument<string>(ArgumentName));

        SetAction(Query);
    }

    private async Task Query(ParseResult parseResult)
    {
        if (!_lifeCycleManager.IsRunning())
        {
            parseResult.Out(ApiResponse.Stopped());
            return;
        }

        var query = parseResult.GetRequiredValue<string>(ArgumentName);
        var name = _config.Name;
        var limit = _config.TopNResults;

        var response = await _client.SendAsync(new Request
        {
            Method = http.HttpMethod.GET,
            Path = "query",
            Query = new Dictionary<string, string>
            {
                { "q", Uri.EscapeDataString(query) },
                { "name", Uri.EscapeDataString(name) },
                { "top", Uri.EscapeDataString(limit.ToString()) }
            }
        });

        parseResult.Out(Format(response.AsApiResponse<Collection<QueryResult>>()));
    }

    private static ApiResponse Format(ApiResponse<Collection<QueryResult>> response)
    {
        var body = (response.Data ?? []).OrderByDescending(x => x.Score);

        return new()
        {
            Status = response.Status,
            Message = @$"{response.Message}        
 {body.Aggregate("", (current, next) => current + $"\r\n<result score='{next.Score}' path='{next.SourcePath}' start='{next.StartOffset}' end='{next.EndOffset}'>\r\n" + next.Content + "\r\n</result>")}"
        };
    }
}