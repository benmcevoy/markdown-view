
namespace ragd.Chunk;

public record ContentChunk
{
    public ContentChunk(string content, string sourcePath, string[] chunkPath, int chunkIndex, int totalChunks, int startOffset, int endOffset, DateTime createdAt, ChunkType chunkType)
    {
        Content = content;
        SourcePath = sourcePath;
        ChunkPath = chunkPath;
        ChunkIndex = chunkIndex;
        TotalChunks = totalChunks;
        StartOffset = startOffset;
        EndOffset = endOffset;
        CreatedAt = createdAt;
        ChunkType = chunkType;
    }

    /// <summary>"Cleaned" Content of this chunk.</summary>
    public string Content { get; init; }
    /// <summary>Original file path/name</summary>
    public string SourcePath { get; init; }
    /// <summary>Hierarchical context from document headings (e.g., ["Chapter 1", "Methods"]).</summary>
    public string[] ChunkPath { get; init; }
    /// <summary>Zero-based index of this chunk within its source document.</summary>
    public int ChunkIndex { get; init; }
    /// <summary>Total number of chunks generated from this source document.</summary>
    public int TotalChunks { get; init; }
    /// <summary>Character offset in the original file where this chunk begins.</summary>
    public int StartOffset { get; init; }
    /// <summary>Character offset in the original file where this chunk ends.</summary>
    public int EndOffset { get; init; }
    public DateTime CreatedAt { get; init; }
    public ChunkType ChunkType { get; init; }


    public static ContentChunk WithChunk(ContentChunk original, string content) =>
       new(content,
           original.SourcePath,
           original.ChunkPath,
           original.ChunkIndex,
           original.TotalChunks,
           original.StartOffset,
           original.EndOffset,
           original.CreatedAt,
           original.ChunkType);

}