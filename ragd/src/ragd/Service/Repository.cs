using Microsoft.Data.Sqlite;
using ragd.Service.Clean;
using ragd.Service.Clean.Markdown;
using ragd.Service.Clean.Text;

namespace ragd.Service;

public interface IRepository
{
    void Dispose();
    void Initialize(int vectorLength);
    ICollection<QueryResult> Query(float[] query, string name, int limit = 10);
    void WriteChunk(ContentChunk chunk, float[] embedding, string name);
}

public class Repository : IDisposable, IRepository
{
    private readonly SqliteConnection _connection;
    private readonly string _databasePath;
    private readonly ICleaner _cleaner;


    public Repository(Config config, QueryResultCleaner cleaner)
    {
        _databasePath = config.DatabasePath;

        _connection = new SqliteConnection($"Data Source={_databasePath}");
        _connection.EnableExtensions();
        _connection.LoadExtension(config.VectorExtensionPath);
        _connection.Open();
        _cleaner = cleaner;

    }

    public void Initialize(int vectorLength)
    {
        if (File.Exists(_databasePath)) File.Delete(_databasePath);

        _connection.Close();

        SqliteConnection.ClearPool(_connection);

        _connection.Open();

        // create schema
        using var cmd = _connection.CreateCommand();

        // notice the vector dimension (vectorLength) is defined by the embedder model
        // also Chunk could be normalized to Document and Chunk tables
        // for now I do not care
        cmd.CommandText = $@"
CREATE VIRTUAL TABLE chunks_vec USING vec0(embedding float[{vectorLength}] distance_metric=cosine);

CREATE TABLE chunks_meta (
    chunks_vec_rowid INTEGER PRIMARY KEY,
    name TEXT NULL,
    content TEXT NOT NULL,
    source_path TEXT NOT NULL,
    chunk_path TEXT NOT NULL,
    chunk_index INTEGER NOT NULL,
    total_chunks INTEGER NOT NULL,
    start_offset INTEGER NOT NULL,
    end_offset INTEGER NOT NULL,
    created_at TEXT NOT NULL
);";
        cmd.ExecuteNonQuery();
    }

    public void WriteChunk(ContentChunk chunk, float[] embedding, string name)
    {
        using var transaction = _connection.BeginTransaction();

        try
        {
            // Insert embedding into vec0 table and retrieve the rowid
            using var vecCmd = _connection.CreateCommand();
            vecCmd.Transaction = transaction;
            vecCmd.CommandText = @"
            INSERT INTO chunks_vec(embedding)
            VALUES (@embedding)
            ";

            // vec0 expects a raw blob of little-endian IEEE 754 floats
            var embeddingBytes = new byte[embedding.Length * sizeof(float)];
            Buffer.BlockCopy(embedding, 0, embeddingBytes, 0, embeddingBytes.Length);
            vecCmd.Parameters.AddWithValue("@embedding", embeddingBytes);

            vecCmd.ExecuteScalar();

            using var rowidCmd = _connection.CreateCommand();
            rowidCmd.Transaction = transaction;
            rowidCmd.CommandText = "SELECT last_insert_rowid()";
            var rowid = (long)rowidCmd.ExecuteScalar()!;

            // Insert chunk metadata with the rowid from the vec0 insert
            using var metaCmd = _connection.CreateCommand();
            metaCmd.Transaction = transaction;
            metaCmd.CommandText = @"
            INSERT INTO chunks_meta (
                chunks_vec_rowid,
                name,
                content,
                source_path,
                chunk_path,
                chunk_index,
                total_chunks,
                start_offset,
                end_offset,
                created_at
            ) VALUES (
                @rowid,
                @name,
                @content,
                @sourcePath,
                @chunkPath,
                @chunkIndex,
                @totalChunks,
                @startOffset,
                @endOffset,
                @createdAt
            )
            ";

            metaCmd.Parameters.AddWithValue("@rowid", rowid);
            metaCmd.Parameters.AddWithValue("@name", name);
            metaCmd.Parameters.AddWithValue("@content", chunk.Content);
            metaCmd.Parameters.AddWithValue("@sourcePath", chunk.SourcePath);
            metaCmd.Parameters.AddWithValue("@chunkPath", string.Join("/", chunk.ChunkPath));
            metaCmd.Parameters.AddWithValue("@chunkIndex", chunk.ChunkIndex);
            metaCmd.Parameters.AddWithValue("@totalChunks", chunk.TotalChunks);
            metaCmd.Parameters.AddWithValue("@startOffset", chunk.StartOffset);
            metaCmd.Parameters.AddWithValue("@endOffset", chunk.EndOffset);
            metaCmd.Parameters.AddWithValue("@createdAt", chunk.CreatedAt.ToString("O")); // ISO 8601

            metaCmd.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public ICollection<QueryResult> Query(float[] query, string name, int limit = 2)
    {
        using var cmd = _connection.CreateCommand();

        var hasName = !string.IsNullOrWhiteSpace(name);

        // sqlite-vec does not support ORDER BY DESC
        // and is unreliable with LIMIT 
        // use k = @limit instead
        cmd.CommandText = $@"
        SELECT chunks_vec.distance,
               chunks_meta.content,  
               chunks_meta.name,
               chunks_meta.source_path,
               chunks_meta.chunk_path,
               chunks_meta.chunk_index,
               chunks_meta.total_chunks,
               chunks_meta.start_offset,
               chunks_meta.end_offset,
               chunks_meta.created_at

        FROM chunks_vec
        INNER JOIN chunks_meta on chunks_meta.chunks_vec_rowid = chunks_vec.rowid
        WHERE chunks_vec.embedding match @embedding
            AND k = @limit
            {(hasName ? "AND chunks_meta.name = @name" : "")}
        ORDER BY chunks_vec.distance ASC
        ";

        var embeddingBytes = new byte[query.Length * sizeof(float)];
        Buffer.BlockCopy(query, 0, embeddingBytes, 0, embeddingBytes.Length);

        cmd.Parameters.AddWithValue("@embedding", embeddingBytes);
        cmd.Parameters.AddWithValue("@limit", limit);

        if (hasName) cmd.Parameters.AddWithValue("@name", name);

        var reader = cmd.ExecuteReader();
        var row = new object[2];
        var result = new List<QueryResult>();

        while (reader.Read())
        {
            var score = 1 / (1 + reader.GetFieldValue<float>(chunks_vec_distance_ordinal));
            var raw = reader.GetFieldValue<string>(chunks_meta_content_ordinal);


            result.Add(new QueryResult(
                raw,
                _cleaner.Clean(raw),
                score,
                reader.GetFieldValue<string>(chunks_meta_name_ordinal),
                reader.GetFieldValue<string>(chunks_meta_source_path_ordinal),
                reader.GetFieldValue<string>(chunks_meta_chunk_path_ordinal),
                reader.GetFieldValue<int>(chunks_meta_chunk_index_ordinal),
                reader.GetFieldValue<int>(chunks_meta_total_chunks_ordinal),
                reader.GetFieldValue<int>(chunks_meta_start_offset_ordinal),
                reader.GetFieldValue<int>(chunks_meta_end_offset_ordinal),
                reader.GetFieldValue<DateTime>(chunks_meta_created_at_ordinal)
            ));
        }

        return result;
    }

    private bool _isDisposing;
    public void Dispose()
    {
        if (_isDisposing) return;

        _isDisposing = true;

        _connection.Dispose();
    }

#pragma warning disable IDE1006 // Naming Styles
    private const int chunks_vec_distance_ordinal = 0;
    private const int chunks_meta_content_ordinal = 1;
    private const int chunks_meta_name_ordinal = 2;
    private const int chunks_meta_source_path_ordinal = 3;
    private const int chunks_meta_chunk_path_ordinal = 4;
    private const int chunks_meta_chunk_index_ordinal = 5;
    private const int chunks_meta_total_chunks_ordinal = 6;
    private const int chunks_meta_start_offset_ordinal = 7;
    private const int chunks_meta_end_offset_ordinal = 8;
    private const int chunks_meta_created_at_ordinal = 9;
#pragma warning restore IDE1006 // Naming Styles
}