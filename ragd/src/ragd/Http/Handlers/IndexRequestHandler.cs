using ragd.Core;
using http;
using System.Collections.ObjectModel;

namespace ragd.Http.Handlers;

public class IndexFileRequestHandler(IIndexer indexer) : IRequestHandler
{
    private readonly IIndexer _indexer = indexer;

    public bool CanHandle(Request request) => request.Path.Equals("index", StringComparison.OrdinalIgnoreCase)
        && request.Method == http.HttpMethod.POST
        && request.Query.ContainsKey("path");

    public IApiResponse<object> Handle(Request request)
    {
        // expect Query to contain file or folder path
        var path = request.Query["path"];
        request.Query.TryGetValue("name", out var name);
        name ??= "";

        return new ApiResponse<Collection<IndexResult>>
        {
            Status = "OK",
            Message = "Indexing complete.",
            Data = _indexer.Index(path, name)
        };
    }
}
