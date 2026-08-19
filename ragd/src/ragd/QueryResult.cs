namespace ragd;

public record QueryResult(
    string Raw,
    string Content,
    float Score,
    string Name,
    string SourcePath,
    string ChunkPath,
    int ChunkIndex,
    int TotalChunks,
    int StartOffset,
    int EndOffset,
    DateTime CreatedAt
);
