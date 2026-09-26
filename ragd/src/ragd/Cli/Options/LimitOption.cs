using System.CommandLine;

namespace ragd.Cli.Options;

/// <summary>
/// Optional Name option, name my be omitted
/// </summary>
public class LimitOption : Option<int>
{
    public const string OptionName = "--top";

    /// <summary>
    /// Optional Name option, name my be omitted
    /// </summary>
    public LimitOption() : base(OptionName, "-n")
    {
        Required = false;
        // TOOD: the number 3 should come from config or somewhere else
        Description = $"Limit number of results returned, (default 3)" ;
    }
}