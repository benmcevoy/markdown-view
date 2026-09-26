using http;
using ragd.Core;

namespace ragd.Http.Handlers;

public class ConfigRequestHandler(Config config) : IRequestHandler
{
    private readonly Config _config = config;

    public bool CanHandle(Request request) =>
        request.Path.Equals("config", StringComparison.OrdinalIgnoreCase)
            && request.Method == http.HttpMethod.GET;

    public IApiResponse<object> Handle(Request request) => new ApiResponse<Config>
    {
        Status = "OK",
        Message = "RAG daemon config",
        Data = _config
    };
}
