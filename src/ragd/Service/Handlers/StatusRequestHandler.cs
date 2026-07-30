using ragd.Http;

namespace ragd.Service.Handlers;

public class StatusRequestHandler(Config config) : IRequestHandler
{
    private readonly Config _config = config;

    public bool CanHandle(Request request) => 
        request.Path.Equals("status", StringComparison.OrdinalIgnoreCase) 
            && request.Method == Http.HttpMethod.GET;

    public Response Handle(Request request) => new(HttpStatusCode.OK)
    {
        Status = LifeCycleStates.RUNNING,
        Message = "Rag Daemon is running",
        Body = new Dictionary<string, string> { { "db", _config.DatabasePath }, { "model", _config.ModelPath } }
    };
}
