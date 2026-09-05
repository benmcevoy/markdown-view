using ragd.Chunk;
using ragd.Embed;
using http;

namespace ragd.Handlers;

public class IndexFileRequestHandler(IEmbedder embedder, IRepository repository, IDocumentChunker chunker) : IRequestHandler
{
    private readonly IEmbedder _embedder = embedder;
    private readonly IRepository _repository = repository;
    private readonly IDocumentChunker _chunker = chunker;

    public bool CanHandle(Request request) => request.Path.Equals("index", StringComparison.OrdinalIgnoreCase)
        && request.Method == http.HttpMethod.POST
        && request.Query.ContainsKey("path");

    public JsonResponse Handle(Request request)
    {
        // expect Query to contain file or folder path
        var path = request.Query["path"];
        
        request.Query.TryGetValue("name", out var name);
        name ??= "";

        var file = new FileInfo(path);

        if (!file.Exists)
        {
            return new JsonResponse<IndexResult> (HttpStatusCode.ClientError, new IndexResult(0, file.FullName, name))
            {
                Status = "ERROR",
                Message = "Unable to find file to index.",
            };
        }

        if (!IsSupportedFileType(file, ".md"))
        {
            return new JsonResponse<IndexResult> (HttpStatusCode.ClientError, new IndexResult(0, file.FullName, name))
            {
                Status = "ERROR",
                Message = $"File of type {file.Extension} is unsupported.",
            };
        }

        try
        {
            var result = IngestDocument(file, name);

            return new JsonResponse<IndexResult> (HttpStatusCode.OK, result)
            {
                Status = "OK",
                Message = $"Successfully indexed {path}",
            };
        }
        catch (ArgumentException ex)
        {
            // ah Exception<> would be handy...
            // type unions are coming
            return new JsonResponse<IndexResult>(HttpStatusCode.ServerError, new IndexResult(0, file.FullName, name))
            {
                Status = "ERROR",
                Message = ex.Message,
            };
        }
    }

    private IndexResult IngestDocument(FileInfo file, string name)
    {
        var content = File.ReadAllText(file.FullName);
        var document = new Document(file.FullName, file.Name, file.Extension, content);
        var chunks = _chunker.Handle(document);

        foreach (var chunk in chunks)
        {
            // make sync
            var embedding = Task.Run(() => _embedder.GetEmbedding(chunk.Content)).GetAwaiter().GetResult();
            _repository.WriteChunk(chunk, embedding, name);
        }

        return new IndexResult(chunks.Count(), file.FullName, name);
    }

    private static bool IsSupportedFileType(FileInfo fi, params string[] extensions)
        => fi.Exists && extensions.Contains(fi.Extension);
}
