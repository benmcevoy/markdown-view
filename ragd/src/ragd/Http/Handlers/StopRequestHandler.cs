using http;
using ragd.Core;

namespace ragd.Http.Handlers;

public class StopRequestHandler : IRequestHandler
{
    public static bool IsStopRequest(Request request) 
        => request.Path.Equals("stop", StringComparison.OrdinalIgnoreCase) 
            && request.Method == http.HttpMethod.POST;

    public bool CanHandle(Request request) => IsStopRequest(request);

    // TODO: this is basically sending a message to the Daemon to stop
    // should use event or pub/sub message or something
    public IApiResponse<object> Handle(Request request) => ApiResponse.Stopped();
}
