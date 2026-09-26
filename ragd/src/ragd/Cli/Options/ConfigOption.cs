using System.CommandLine;

namespace ragd.Cli.Options;

public class ConfigOption : Option<string>
{
    public const string OptionName = "--config";

    public ConfigOption() : base(OptionName)
    {
        Required = true;
    }
}
