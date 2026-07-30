
namespace ragd.Service
{
    public record ContentChunk(
        /// <summary>"Cleaned" Content of this chunk.</summary>
        string Content,
        /// <summary>Original file path/name</summary>
        string SourcePath,
        /// <summary>Hierarchical context from document headings (e.g., ["Chapter 1", "Methods"]).</summary>
        string[] ChunkPath,
        /// <summary>Zero-based index of this chunk within its source document.</summary>
        int ChunkIndex,
        /// <summary>Total number of chunks generated from this source document.</summary>
        int TotalChunks,
        /// <summary>Character offset in the original file where this chunk begins.</summary>
        int StartOffset,
        /// <summary>Character offset in the original file where this chunk ends.</summary>
        int EndOffset,
        DateTime CreatedAt
    );
}