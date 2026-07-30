using ragd.Http;

namespace ragd.Service.Handlers;

public class StopRequestHandler : IRequestHandler
{
    public static bool IsStopRequest(Request request) 
        => request.Path.Equals("stop", StringComparison.OrdinalIgnoreCase) 
            && request.Method == Http.HttpMethod.POST;

    public bool CanHandle(Request request) => IsStopRequest(request);

    public Response Handle(Request request) => new(HttpStatusCode.OK)
    {
        Status = LifeCycleStates.STOPPED,
        Message = "Rag Daemon is stopping"
    };
}
