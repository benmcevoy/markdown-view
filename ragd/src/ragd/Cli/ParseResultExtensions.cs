using System.CommandLine;
using System.Text.Json;
using ragd.Cli.Options;
using ragd.Core;

namespace ragd.Cli;

public static class ParseResultExtensions
{
    private static JsonSerializerOptions _serializerOptions = new(JsonSerializerOptions.Web) { WriteIndented = true };

    public static bool IsQuiet(this ParseResult parseResult)
        => parseResult.GetValue<bool>(QuietOption.OptionName) || parseResult.IsJson();
    private static bool IsJson(this ParseResult parseResult) => parseResult.GetValue<bool>(JsonOption.OptionName);

    /// <summary>
    /// Write response to the caller via std out. 
    /// </summary>
    /// <remarks>--quiet has no effect</remarks>
    public static void Out(this ParseResult parseResult, ApiResponse response) =>
        Console.WriteLine(parseResult.IsJson()
            ? AsJson(response)
            : AsText(response));

    public static void Out(this ParseResult parseResult, string message) => Console.WriteLine(message);

    public static void Log(this ParseResult parseResult, string message)
    {
        if (parseResult.IsQuiet()) return;

        Console.WriteLine(message);
    }

    private static string AsJson(ApiResponse response)
    {
        if (response.Data is null)
        {
            return JsonSerializer.Serialize(new { response.Status, response.Message }, _serializerOptions);
        }

        return JsonSerializer.Serialize(new { response.Status, response.Message, response.Data }, _serializerOptions);
    }

    private static string AsText(ApiResponse response)
        =>  @$"{response.Status} - {response.Message}";
}

