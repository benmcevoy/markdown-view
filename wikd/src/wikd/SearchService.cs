using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace wikd;

public class SearchService
{
    private readonly string _collectionName;
    private readonly string _cliPath;
    private readonly bool _isInitialised;

    public SearchService(string collectionName)
    {
        _collectionName = collectionName;

        if (!TryFindCliPath("ragd", out _cliPath)) 
            throw new InvalidOperationException("ragd cli tool cannot be found. Is it on the PATH?");

        var status = Status();

        _isInitialised = status.Equals("RUNNING", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsSearchAvailable() => _isInitialised;

    public string Search(string query)
    {
        var result = Execute($"query \"{query}\" --json --name \"{_collectionName}\"");

        return result.Body.ToString();
    }

    private bool TryFindCliPath(string cliTool, out string cliPath)
    {
        cliPath = "";

        var path = Environment.GetEnvironmentVariable("PATH");
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var separator = isWindows ? ';' : ':';
        var paths = path!.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var p in paths)
        {
            cliPath = Path.Combine(p, cliTool);

            if (File.Exists(cliPath)) return true;

            cliPath = Path.Combine(p, cliTool, ".exe");

            if (File.Exists(cliPath)) return true;

            cliPath = Path.Combine(p, cliTool, ".dll");

            if (File.Exists(cliPath))
            {
                cliPath = $"dotnet \"{cliPath}\"";
                return true;
            }
        }

        return false;
    }

    private string Status()
    {
        var result = Execute("status --json");

        return result.Status;
    }

    private static void Index()
    {
        throw new NotImplementedException("TODO:");
    }

    private ApiResult Execute(string command)
    {
        // TODO: i should probably just be using the tcp socket
        // it would be faster
        var psi = new ProcessStartInfo
        {
            RedirectStandardOutput = true,
            FileName = _cliPath,
            Arguments = command,
            UseShellExecute = false
        };

        using var proc = Process.Start(psi);

        var result = proc!.StandardOutput.ReadToEnd();

        proc.WaitForExit();

        return JsonSerializer.Deserialize(result, SearchServiceJsonContext.Default.ApiResult) ?? new ApiResult();
    }

    public record ApiResult
    {
        public string Status { get; set; } = "";
        public object Body { get; set; } = new();
        public string Message { get; set; } = "";
    }
}

// SearchServiceJsonContext is source generated, requires a build to create the implementation
// can then use SearchServiceJsonContext.Default.MyType
// This class cannot be nested inside SearchService as that 
// prevents SearchService from building... chicken and egg... circular dependancy
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(SearchService.ApiResult))]
internal partial class SearchServiceJsonContext : JsonSerializerContext { }
