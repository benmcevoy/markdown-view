using System.CommandLine;
using System.Text.Json;
using ragd.Cli.Options;
using ragd.Core;

namespace ragd.Cli;

public static class ParseResultExtensions
{
    public static bool IsQuiet(this ParseResult parseResult)
        => parseResult.GetValue<bool>(QuietOption.OptionName) || parseResult.IsJson();
    private static bool IsJson(this ParseResult parseResult) => parseResult.GetValue<bool>(JsonOption.OptionName);

    /// <summary>
    /// Write response to the caller via std out. 
    /// </summary>
    /// <remarks>--quiet has no effect</remarks>
    public static void Out(this ParseResult parseResult, ApiResponse response, Func<ApiResponse, string>? formatter = default) =>
        Console.WriteLine(parseResult.IsJson()
            ? AsJson(response)
            : AsText(response, formatter));

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
            return JsonSerializer.Serialize(new { response.Status, response.Message }, new JsonSerializerOptions(JsonSerializerOptions.Web) { WriteIndented = true });
        }

        return JsonSerializer.Serialize(new { response.Status, response.Message, response.Data }, new JsonSerializerOptions(JsonSerializerOptions.Web) { WriteIndented = true });
    }

    private static string AsText(ApiResponse response, Func<ApiResponse, string>? formatter)
        => (formatter is null)
            ? @$"{response.Status} - {response.Message}"
            : formatter(response);
}

