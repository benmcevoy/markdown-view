using System.CommandLine;
using ragd.Core;
using ragd.Cli.Options;
using ragd.Core.Embed;

namespace ragd.Cli;

public class NewCommand : Command
{
    private readonly Config _config;

    public NewCommand(Config config) : base("new", "Create a new sqlite embedding database.")
    {
        Options.Add(new DatabaseOption(databaseMustExistIsOptional: true));
        Options.Add(new ModelOption());

        SetAction(New);
        _config = config;

    }

    private void New(ParseResult parseResult)
    {
        var database = new FileInfo(_config.DatabasePath);
        var model = new FileInfo(_config.ModelPath);

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
        using var repository = new Repository(config, new());

        var vectorLength = embedder.EmbeddingSize();
        var trainedContextSize = embedder.TrainedContextSize();

        parseResult.Log($"Creating new embedding database with vector length '{vectorLength}' at '{database.FullName}'.");
        parseResult.Log($"Model has trained context size of '{trainedContextSize}'.");

        repository.CreateNewDatabase(vectorLength);
        parseResult.Out(Created(database.FullName, model.FullName));
    }

    private static ApiResponse Created(string database, string model) =>
        new()
        {
            Status = "OK",
            Message = @$"Database created. You can start the daemon with: 
        
rag start --database '{database}' --model '{model}'"
        };
}
