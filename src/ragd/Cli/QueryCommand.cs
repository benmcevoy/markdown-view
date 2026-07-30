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

    public QueryCommand(LifeCycleManager lifeCycleManager, Client client, ILogger logger)
        : base("query", "Query the index for semantically matching results.")
    {
        _lifeCycleManager = lifeCycleManager;
        _client = client;
        _logger = logger;

        Options.Add(new NameOption());
        Arguments.Add(new Argument<string>(ArgumentName));

        SetAction(Query);
    }

    private void Query(ParseResult parseResult)
    {
        if (!_lifeCycleManager.IsRunning())
        {
            parseResult.Out(Response.DaemonNotRunning);
            return;
        }

        var name = parseResult.GetValue<string>(NameOption.OptionName) ?? "";
        var query = parseResult.GetRequiredValue<string>(ArgumentName);

        var response = _client.Send(new Request
        {
            Method = Http.HttpMethod.GET,
            Path = "query",
            Query = new Dictionary<string, string>
            {
                { "q", Uri.EscapeDataString(query) },
                { "name", Uri.EscapeDataString(name) }
            }
        });

        parseResult.Out(response, Format);
    }

    private static string Format(ParseResult parseResult, Response response)
    {
        if (parseResult.IsJson()) return Response.AsJson(response);

        var body = response.BodyAs<Collection<QueryResult>>() ?? [];

        return @$"Status: {response.Status}
Message: {response.Message}        
{body.Aggregate("", (current, next) => current + "\r\n<result>\r\n" + next.Content + "\r\n</result>")}";
    }
}