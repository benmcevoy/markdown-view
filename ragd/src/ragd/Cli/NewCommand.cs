using System.CommandLine;
using Microsoft.Extensions.Logging;
using ragd.Cli.Options;
// TODO: something wrong here
// new database is not part of service, exists on the cli side
// I think this should delegate to a Service.NewHandler
using ragd.Service;
using ragd.Service.Embed;

namespace ragd.Cli;

public class NewCommand : Command
{
    private readonly ILogger _logger;

    public NewCommand(ILogger logger) : base("new", "Create a new sqlite embedding database.")
    {
        _logger = logger;

        Options.Add(new DatabaseOption(true));
        Options.Add(new ModelOption());

        SetAction(New);
    }

    private void New(ParseResult parseResult)
    {
        var database = parseResult.GetRequiredValue<FileInfo>(DatabaseOption.OptionName);
        var model = parseResult.GetRequiredValue<FileInfo>(ModelOption.OptionName);

        // TODO: unhappy that I have not injected this
        // the embedder is a heavyweight object
        // would require e.g. a Load() method and state to check if model 
        // is loaded before using the embedder
        // maybe Lazy? can work? 
        // needs service provider scope for cli vs daemon
        var config = new Config
        {
            DatabasePath = database.FullName,
            ModelPath = model.FullName,
            VectorExtensionPath = Path.Combine(AppContext.BaseDirectory, "vec0.so")
        };
        using var embedder = new Embedder(config);
        using var repository = new Repository(config);

        var vectorLength = embedder.EmbeddingSize();
        var trainedContextSize = embedder.TrainedContextSize();

        _logger.LogInformation($"Creating new embedding database with vector length '{vectorLength}' at '{database.FullName}'.");
        _logger.LogInformation($"Configured context size is '{config.ContextSize}'.");
        _logger.LogInformation($"Model has trained context size of '{trainedContextSize}'.");

        repository.Initialize(vectorLength);

        _logger.LogInformation(@$"You can start the daemon with: 
        
rag start --database '{database.FullName}' --model '{model.FullName}'");

        parseResult.Out("State: Created");
    }
}
