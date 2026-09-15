using ragd.Core;

namespace ragd.Http.Handlers;

public interface IRequestHandler : IHandler<http.Request, IApiResponse<object>>;
