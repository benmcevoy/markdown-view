using System.CommandLine;
using System.CommandLine.Parsing;
using ragd.Cli.Options;
using http;
using ragd.Core;

namespace ragd.Cli;

public class IndexCommand : Command
{
    private readonly Config _config;
    private readonly LifeCycleManager _lifeCycleManager;
    private readonly Client _client;
    private const string ArgumentName = "path";

    public IndexCommand(Config config, LifeCycleManager lifeCycleManager, Client client)
        : base("index", "Index the given file or folder.")
    {
        _config = config;
        _lifeCycleManager = lifeCycleManager;
        _client = client;

        var argument = new Argument<string>(ArgumentName);

        argument.Validators.Add(Validate);

        Options.Add(new NameOption());
        Arguments.Add(argument);

        SetAction(Index);
    }

    private async Task Index(ParseResult parseResult)
    {
        if (!_lifeCycleManager.IsRunning())
        {
            parseResult.Out(DaemonNotRunning());
            return;
        }

        var name = _config.Name;
        var path = parseResult.GetRequiredValue<string>(ArgumentName);

        var response = await _client.SendAsync(
            new Request
            {
                Method = http.HttpMethod.POST,
                Path = "index",
                // TODO: should this be body? POST with query string feels wrong
                Query = new Dictionary<string, string>
                {
                    { "path", Uri.EscapeDataString(path) },
                    { "name", Uri.EscapeDataString(name) }
                }
            });

        parseResult.Out(response.AsApiResponse());
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

    private static ApiResponse DaemonNotRunning() =>
         new() { Message = "Daemon not running.", Status = "ERROR" };
}
