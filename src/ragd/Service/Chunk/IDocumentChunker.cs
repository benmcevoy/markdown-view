namespace ragd.Service.Chunk;
interface IDocumentChunker : IHandler<Document, IEnumerable<ContentChunk>>;