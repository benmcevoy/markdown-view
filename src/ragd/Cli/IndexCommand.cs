using System.Collections.ObjectModel;
using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Logging;
using ragd.Cli.Options;
using ragd.Http;
using ragd.Service.Handlers;

namespace ragd.Cli;

public class IndexCommand : Command
{
    private readonly LifeCycleManager _lifeCycleManager;
    private readonly Client _client;
    private readonly ILogger _logger;
    private const string ArgumentName = "path";

    public IndexCommand(LifeCycleManager lifeCycleManager, Client client, ILogger logger)
        : base("index", "Index the given file or folder.")
    {
        _lifeCycleManager = lifeCycleManager;
        _client = client;
        _logger = logger;

        var argument = new Argument<string>(ArgumentName);

        argument.Validators.Add(Validate);

        Options.Add(new NameOption());
        Arguments.Add(argument);

        SetAction(Index);
    }

    private void Index(ParseResult parseResult)
    {
        if (!_lifeCycleManager.IsRunning())
        {
            parseResult.Out(Response.DaemonNotRunning);
            return;
        }

        var name = parseResult.GetValue<string>(NameOption.OptionName) ?? "";
        var path = parseResult.GetRequiredValue<string>(ArgumentName);
        var paths = GetFilePaths(path);
        var results = new Collection<IndexResult>();
        var count = paths.Length;
        var i = 0;

        _logger.LogInformation($"Found {count} files to index.");

        foreach (var filePath in paths)
        {
            i++;
            _logger.LogInformation($"Started {i} of {count} ({filePath.FullName})...");

            var response = _client.Send(
                new Request
                {
                    Method = Http.HttpMethod.POST,
                    Path = "index",
                    // TODO: should this be body? POST with query string feels wrong
                    Query = new Dictionary<string, string>
                    {
                        { "path", Uri.EscapeDataString(filePath.FullName) },
                        { "name", Uri.EscapeDataString(name) }
                    }
                });

            parseResult.Out(response, Format);
        }
    }

    private void Validate(ArgumentResult result)
    {
        var value = result.GetValue<string>(result.Argument.Name) ?? "";

        if (string.IsNullOrWhiteSpace(value))
        {
            result.AddError($"{result.Argument.Name} is required.");
        }

        if (!FileOrFolderExists(value))
        {
            result.AddError($"Unable to find file or folder path to index.");
        }
    }

    private static bool FileOrFolderExists(string path) =>
        new FileInfo(path).Exists || new DirectoryInfo(path).Exists;

    private static string Format(ParseResult parseResult, Response response)
    {
        if (parseResult.IsJson()) return Response.AsJson(response);

        var body = response.BodyAs<IndexResult>() ?? new(0, "ERROR", "ERROR");

        return @$"Status: {response.Status}
Message: {response.Message}        
Path: {body.FilePath}
Chunks: {body.ChunkCount}
Name: {body.Name}";
    }

    private static FileInfo[] GetFilePaths(string root)
    {
        var fi = new FileInfo(root);

        if (fi.Exists ) return [fi];

        var di = new DirectoryInfo(root);

        if (di.Exists)
        {
            return di.GetFiles("*.*", SearchOption.AllDirectories);
        }

        return [];
    }
}
