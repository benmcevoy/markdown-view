using System.CommandLine;

namespace ragd.Cli.Options;

/// <summary>
/// Optional Name option, name my be omitted
/// </summary>
public class NameOption : Option<string>
{
    public const string OptionName = "--name";

    /// <summary>
    /// Optional Name option, name my be omitted
    /// </summary>
    public NameOption() : base(OptionName, "-n")
    {
        Description = "Optional name for a collection of embeddings";
        DefaultValueFactory = x => "";
        Required = false;
    }
}