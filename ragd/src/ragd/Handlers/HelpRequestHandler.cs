using System.Reflection;
using ragd.Http;

namespace ragd.Handlers;

public class HelpRequestHandler : IRequestHandler
{
    private static readonly string _api = GetResource("ragd.Handlers.help.json");

    public bool CanHandle(Request request) => request.Path.Equals("help", StringComparison.OrdinalIgnoreCase)
            && request.Method == Http.HttpMethod.GET;

    public JsonResponse Handle(Request request) => new(HttpStatusCode.OK) { Status = "OK", Body = _api };

    private static string GetResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resource = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(resource!);

        return reader.ReadToEnd();
    }
}