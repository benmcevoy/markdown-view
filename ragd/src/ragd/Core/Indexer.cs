using System.Collections.ObjectModel;
using ragd.Core.Chunk;
using ragd.Core.Embed;

namespace ragd.Core;

public interface IIndexer
{
    Collection<IndexResult> Index(string path, string name);
}

public class Indexer : IIndexer
{
    private readonly IEmbedder _embedder;
    private readonly IRepository _repository;
    private readonly IDocumentChunker _chunker;

    public Indexer(IEmbedder embedder, IRepository repository, IDocumentChunker chunker)
    {
        _embedder = embedder;
        _repository = repository;
        _chunker = chunker;
    }

    public Collection<IndexResult> Index(string path, string name)
    {
        var results = new Collection<IndexResult>();

        if (!FileOrFolderExists(path))
        {
            results.Add(new IndexResult(0, path, name, "Unable to find file or folder to index."));

            return results;
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

        return results;
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