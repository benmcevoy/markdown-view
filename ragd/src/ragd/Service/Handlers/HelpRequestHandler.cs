using ragd.Http;

namespace ragd.Service.Handlers;

public class HelpRequestHandler : IRequestHandler
{
    public bool CanHandle(Request request) => request.Path.Equals("help", StringComparison.OrdinalIgnoreCase)
            && request.Method == Http.HttpMethod.GET;

    public JsonResponse Handle(Request request) => new(HttpStatusCode.OK) { Status = "OK", Body = Help.Api };
}