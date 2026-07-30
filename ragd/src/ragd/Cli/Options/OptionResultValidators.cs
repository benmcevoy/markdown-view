using System.CommandLine.Parsing;

namespace ragd.Cli.Options;

public class OptionResultValidators
{
    public static void FileExists(OptionResult result)
    {
        var fi = result.GetValue<FileInfo>(result.Option.Name);

        // if option was NOT required but fileinfo provided
        if (!result.Option.Required && fi != null && !fi.Exists)
        {
            result.AddError($"Option '{result.Option.Name}' file '{fi!.FullName}' not found.");
            return;
        }

        // option is required so forgive null
        if (!fi!.Exists)
        {
            result.AddError($"Option '{result.Option.Name}' file '{fi!.FullName}' not found.");
        }
    }
}