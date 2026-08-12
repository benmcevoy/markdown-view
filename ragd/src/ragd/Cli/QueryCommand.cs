using System.Collections.ObjectModel;
using System.CommandLine;
using Microsoft.Extensions.Logging;
using ragd.Cli.Options;
using ragd.Http;
using ragd.Service;

namespace ragd.Cli;

public class QueryCommand : Command
{
    private readonly LifeCycleManager _lifeCycleManager;
    private readonly Client _client;
    private readonly ILogger _logger;
    private const string ArgumentName = "query";
    private const string LimitOptionName = "--top";
    private const int LimitDefault = 3;

    public QueryCommand(LifeCycleManager lifeCycleManager, Client client, ILogger logger)
        : base("query", "Query the index for semantically matching results.")
    {
        _lifeCycleManager = lifeCycleManager;
        _client = client;
        _logger = logger;

        Options.Add(new NameOption());
        Options.Add(new Option<int>(LimitOptionName) { Required = false, Description = $"Limit number of results returned, (default {LimitDefault})" });
        Arguments.Add(new Argument<string>(ArgumentName));

        SetAction(Query);
    }

    private void Query(ParseResult parseResult)
    {
        if (!_lifeCycleManager.IsRunning())
        {
            parseResult.Out(JsonResponse.DaemonNotRunning);
            return;
        }

        var name = parseResult.GetValue<string>(NameOption.OptionName) ?? "";
        var query = parseResult.GetRequiredValue<string>(ArgumentName);
        var limit = parseResult.GetValue<int?>(LimitOptionName) ?? LimitDefault;

        var response = _client.Send(new Request
        {
            Method = Http.HttpMethod.GET,
            Path = "query",
            Query = new Dictionary<string, string>
            {
                { "q", Uri.EscapeDataString(query) },
                { "name", Uri.EscapeDataString(name) },
                { "top", Uri.EscapeDataString(limit.ToString()) }
            }
        });

        parseResult.Out(response, Format);
    }

    private static string Format(ParseResult parseResult, JsonResponse response)
    {
        if (parseResult.IsJson()) return JsonResponse.AsJson(response);

        var body = (response.BodyAs<Collection<QueryResult>>() ?? [])
                .OrderByDescending(x => x.Score);

        return @$"Status: {response.Status}
Message: {response.Message}        
{body.Aggregate("", (current, next) => current + $"\r\n<result score='{next.Score}' path='{next.SourcePath}' start='{next.StartOffset}' end='{next.EndOffset}'>\r\n" + next.Content + "\r\n</result>")}";
    }
}