using System.CommandLine;

namespace ragd.Cli.Options;

public class QuietOption : Option<bool>
{
    public const string OptionName = "--quiet";
    public QuietOption() : base(OptionName, "-q")
    {
        Description = "Suppress all non essential output.";
        Required = false;
    }
}
