using System.CommandLine;
using ragd.Cli.Options;
using ragd.Http;

namespace ragd.Cli;

public static class ParseResultExtensions
{
    public static bool IsQuiet(this ParseResult parseResult) => parseResult.GetValue<bool>(QuietOption.OptionName);
    public static bool IsJson(this ParseResult parseResult) => parseResult.GetValue<bool>(JsonOption.OptionName);
    
    /// <summary>
    /// Write response to the caller via std out 
    /// </summary>
    public static void Out(this ParseResult _, string response) => Console.WriteLine(response);
    
    /// <summary>
    /// Write response to the caller via std out. 
    /// </summary>
    public static void Out(this ParseResult parseResult, Response response) =>
        Console.WriteLine(parseResult.IsJson()
            ? Response.AsJson(response)
            : Response.AsText(response));

    public static void Out(this ParseResult parseResult, Response response, Func<ParseResult, Response, string> formatter) =>
        Console.WriteLine(formatter(parseResult, response));            
}
