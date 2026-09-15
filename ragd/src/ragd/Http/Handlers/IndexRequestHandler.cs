using ragd.Core;
using ragd.Core.Chunk;
using ragd.Core.Embed;
using http;
using System.Collections.ObjectModel;

namespace ragd.Http.Handlers;

public class IndexFileRequestHandler(IEmbedder embedder, IRepository repository, IDocumentChunker chunker) : IRequestHandler
{
    private readonly IEmbedder _embedder = embedder;
    private readonly IRepository _repository = repository;
    private readonly IDocumentChunker _chunker = chunker;

    public bool CanHandle(Request request) => request.Path.Equals("index", StringComparison.OrdinalIgnoreCase)
        && request.Method == http.HttpMethod.POST
        && request.Query.ContainsKey("path");

    public IApiResponse<object> Handle(Request request)
    {
        // expect Query to contain file or folder path
        var path = request.Query["path"];
        var results = new Collection<IndexResult>();

        request.Query.TryGetValue("name", out var name);
        name ??= "";

        if (!FileOrFolderExists(path))
        {
            results.Add(new IndexResult(0, path, name, "Unable to find file or folder to index."));

            return new ApiResponse<Collection<IndexResult>>
            {
                Data = results,
                Status = "ERROR",
                Message = "Unable to find file or folder to index.",
            };
        }

        var files = GetFilePaths(path);

        foreach (var file in files)
        {
            if (!IsSupportedFileType(file, ".md"))
            {
                results.Add(new IndexResult(0, file.FullName, name, $"File of type {file.Extension} is unsupported."));
                continue;
            }

            try
            {
                results.Add(IngestDocument(file, name));
            }
            catch (ArgumentException ex)
            {
                // ah Exception<> would be handy...
                // type unions are coming
                results.Add(new IndexResult(0, file.FullName, name, ex.Message));
            }
        }

        return new ApiResponse<Collection<IndexResult>>
        {
            Status = "OK",
            Message = "Indexing complete.",
            Data = results
        };
    }

    // TODO: this code belongs in Core - Indexing responsibility
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

        return new IndexResult(chunks.Count(), file.FullName, name, $"Successfully indexed.");
    }

    private static bool IsSupportedFileType(FileInfo fi, params string[] extensions)
        => fi.Exists && extensions.Contains(fi.Extension);

    private static bool FileOrFolderExists(string path) =>
            new FileInfo(path).Exists || new DirectoryInfo(path).Exists;

    private static FileInfo[] GetFilePaths(string root)
    {
        var fi = new FileInfo(root);

        if (fi.Exists) return [fi];

        var di = new DirectoryInfo(root);

        if (di.Exists)
        {
            return di.GetFiles("*.*", SearchOption.AllDirectories);
        }

        return [];
    }
}
