using System.CommandLine;

namespace ragd.Cli.Options;

public class ModelOption : Option<FileInfo>
{
    public const string OptionName = "--model";

    public ModelOption() : base(OptionName, "-m")
    {
        Description = "Path to embedding model, e.g. './bge-small-en.gguf'";
        Required = true;
        AcceptLegalFilePathsOnly();
        Validators.Add(OptionResultValidators.FileExists);
    }
}
