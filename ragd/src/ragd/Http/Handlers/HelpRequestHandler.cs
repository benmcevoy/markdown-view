using System.Reflection;
using http;
using ragd.Core;

namespace ragd.Http.Handlers;

public class HelpRequestHandler : IRequestHandler
{
    private static readonly string _api = GetResource("ragd.Http.Handlers.help.json");

    public bool CanHandle(Request request) => request.Path.Equals("help", StringComparison.OrdinalIgnoreCase)
            && request.Method == http.HttpMethod.GET;

    public IApiResponse<object> Handle(Request request) => new ApiResponse<string>
    {
        Status = "OK",
        Data = _api
    };

    private static string GetResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resource = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(resource!);

        return reader.ReadToEnd();
    }
}