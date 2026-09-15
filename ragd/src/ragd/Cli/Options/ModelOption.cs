using System.CommandLine;

namespace ragd.Cli.Options;

public class ModelOption : Option<FileInfo>
{
    public const string OptionName = "--model";

    public ModelOption(bool optional = false) : base(OptionName, "-m")
    {
        Description = "Path to embedding model, e.g. './bge-small-en.gguf'";
        Required = !optional;
        AcceptLegalFilePathsOnly();
        Validators.Add(OptionResultValidators.FileExists);
    }
}
