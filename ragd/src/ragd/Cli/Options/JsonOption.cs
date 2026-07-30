using System.CommandLine;

namespace ragd.Cli.Options;

public class JsonOption : Option<bool>
{
    public const string OptionName = "--json";
    public JsonOption() : base(OptionName)
    {
        Description = "Return results as JSON.";
        Required = false;
    }
}
