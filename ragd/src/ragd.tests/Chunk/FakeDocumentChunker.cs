using ragd.Chunk;
using ragd.Embed;
namespace ragd.Tests.Chunk;

class FakeDocumentChunker(ChunkType chunkType, params string[] content) : IDocumentChunker
{
    public bool CanHandle(Document context) => true;
    public IEnumerable<ContentChunk> Handle(Document context) => content.Select(C);
    private ContentChunk C(string content) => new(content, "", [], 0, 0, 0, 0, DateTime.Now, chunkType);
}

class FakeEmbedder(int trainedContextSize) : IEmbedder
{
    public int EmbeddingSize() => 768;
    public Task<float[]> GetEmbedding(string content) => throw new NotImplementedException();
    public int TrainedContextSize() => trainedContextSize;
}