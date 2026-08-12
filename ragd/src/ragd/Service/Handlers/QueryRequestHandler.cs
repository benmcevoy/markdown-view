using ragd.Service.Clean;
using ragd.Service.Clean.Text;
using ragd.Service.Embed;
using ragd.Http;

namespace ragd.Service.Handlers;

public class QueryRequestHandler(IRepository repository, IEmbedder embedder, CondenseWhiteSpaceCleaner cleaner) : IRequestHandler
{
    private readonly IRepository _repository = repository;
    private readonly IEmbedder _embedder = embedder;
    private readonly ICleaner _cleaner = cleaner;
    private const int LimitDefault = 3;

    public bool CanHandle(Request request) => request.Path.Equals("query", StringComparison.OrdinalIgnoreCase)
        && request.Method == Http.HttpMethod.GET
        && request.Query.ContainsKey("q");

    public JsonResponse Handle(Request request)
    {
        var query = request.Query["q"];
        request.Query.TryGetValue("name", out var name);
        var cleanQuery = _cleaner.Clean(query);

        request.Query.TryGetValue("top", out var top);
        if (!int.TryParse(top, out var limit)) limit = LimitDefault;

        // make sync
        var embedding = Task.Run(() => _embedder.GetEmbedding(cleanQuery)).GetAwaiter().GetResult();
        var results = _repository.Query(embedding, name ?? "", limit);

        return new(HttpStatusCode.OK)
        {
            Body = results,
            Status = "OK",
            Message = $"Found {results.Count} results."
        };
    }
}
