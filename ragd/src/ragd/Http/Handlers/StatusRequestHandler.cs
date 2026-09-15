using http;
using ragd.Core;

namespace ragd.Http.Handlers;

public class StatusRequestHandler(Config config) : IRequestHandler
{
    private readonly Config _config = config;

    public bool CanHandle(Request request) =>
        request.Path.Equals("status", StringComparison.OrdinalIgnoreCase)
            && request.Method == http.HttpMethod.GET;

    public IApiResponse<object> Handle(Request request) => new ApiResponse<Dictionary<string, string>>
    {
        Status = LifeCycleStates.RUNNING,
        Message = "RAG daemon is running",
        Data = new Dictionary<string, string> { { "db", _config.DatabasePath }, { "model", _config.ModelPath } }
    };
}
