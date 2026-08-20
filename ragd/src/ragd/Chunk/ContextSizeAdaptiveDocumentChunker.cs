namespace ragd.Chunk;

using System.Text.RegularExpressions;
using ragd.Embed;


public partial class ContextSizeAdaptiveDocumentChunker(IDocumentChunker inner, IEmbedder embedder) : IDocumentChunker
{
    private const float ContextSizeProportion = 0.8f;
    private const float CharactersPerToken = 3.4f;
    private readonly IDocumentChunker _inner = inner;
    public readonly int CharacterLimit = Convert.ToInt32(ContextSizeProportion * embedder.TrainedContextSize() * CharactersPerToken);

    // recursive named delegate.  Using a Func<> instead of a delegate is not possible (AFAIK)
    // this information came courtesy of Claude
    // my own analysis had yeilded something like:
    // (IEnumerable<ContentChunk>, Func<ContentChunk, IEnumerable<ContentChunk>>) Paragraph(ContentChunk chunk)
    // but oh my... that did a lot weird things once you got more than one function in the chain
    private delegate (IEnumerable<ContentChunk> Chunks, Generator? Next) Generator(ContentChunk chunk);

    public bool CanHandle(Document context) => _inner.CanHandle(context);
    public IEnumerable<ContentChunk> Handle(Document context) => Split(_inner.Handle(context), ParagraphGenerator);

    private IEnumerable<ContentChunk> Split(IEnumerable<ContentChunk> chunks, Generator generator)
    {
        // TODO: should be accumulating the sub chunks to fit closer to token limit
        // for paragraph... not too sure
        // push that responsibility to the generator

        foreach (var chunk in chunks)
        {
            if (WithinLimit(chunk))
            {
                yield return chunk;
                continue;
            }

            var (subChunks, next) = generator(chunk);
            var accumulator = ContentChunk.WithChunk(chunk, "");

            if (next is null)
            {
                // just yeet, cannot break it down any more
                foreach (var subChunk in subChunks)
                {
                    if (WithinLimit(accumulator.Content.Length + subChunk.Content.Length))
                    {
                        accumulator = Append(accumulator, subChunk);
                        continue;
                    }

                    yield return accumulator;
                    accumulator = subChunk;
                }

                if (!string.IsNullOrWhiteSpace(accumulator.Content)) yield return accumulator;

                continue;
            }

            accumulator = ContentChunk.WithChunk(chunk, "");

            foreach (var subChunk in Split(subChunks, next))
            {
                if (WithinLimit(accumulator.Content.Length + subChunk.Content.Length))
                {
                    accumulator = Append(accumulator, subChunk);
                    continue;
                }

                yield return accumulator;
                accumulator = subChunk;
            }

            if (!string.IsNullOrWhiteSpace(accumulator.Content)) yield return accumulator;
        }
    }

    private ContentChunk Append(ContentChunk accumulator, ContentChunk subChunk) =>
        ContentChunk.WithChunk(accumulator, $"{accumulator.Content} {subChunk.Content}".Trim());

    private bool WithinLimit(ContentChunk chunk) => WithinLimit(chunk.Content);
    private bool WithinLimit(string content) => WithinLimit(content.Length);
    private bool WithinLimit(int length) => length <= CharacterLimit;

    // ATTR: regex nicked from LangChain4j
    [GeneratedRegex(@"\s*(?>\r\n|\r|\n)\s*(?>\r\n|\r|\n)\s*")] private static partial Regex _paragraphSplitRegex();
    private static (IEnumerable<ContentChunk>, Generator) ParagraphGenerator(ContentChunk chunk)
    {
        var paragraphs = _paragraphSplitRegex().Split(chunk.Content);

        switch (chunk.ChunkType)
        {
            case ChunkType.Markdown:
                {
                    var mdParagraphs = new List<string>();
                    var accumulator = "";

                    foreach (var p in paragraphs)
                    {
                        if (IsMarkdownHeading(p))
                        {
                            accumulator = p;
                            continue;
                        }

                        mdParagraphs.Add($"{accumulator}\n\n{p}".Trim());
                        accumulator = "";
                    }

                    if (!string.IsNullOrWhiteSpace(accumulator)) mdParagraphs.Add(accumulator);

                    return (mdParagraphs.Select(c => ContentChunk.WithChunk(chunk, c)), SentanceGenerator);
                }

            default: return (paragraphs.Select(c => ContentChunk.WithChunk(chunk, c)), SentanceGenerator);
        }
    }

    private static bool IsMarkdownHeading(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return false;
        if (content.StartsWith("# ")) return true;
        if (content.StartsWith("## ")) return true;
        if (content.StartsWith("###")) return true;

        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (lines[1].StartsWith("===") || lines[1].StartsWith("---")) return true;

        return false;
    }

    // ATTR: and this is from Claude
    [GeneratedRegex(@"[^.?!;]+[.?!;]+|[^.?!;]+$")] private static partial Regex _sentanceSplitRegex();
    private static (IEnumerable<ContentChunk>, Generator?) SentanceGenerator(ContentChunk chunk) =>
    (_sentanceSplitRegex()
        .Matches(chunk.Content)
        .Where(m => !string.IsNullOrWhiteSpace(m.Value))
        .Select(c => ContentChunk.WithChunk(chunk, c.Value.Trim())), null);

    // TODO: chop up by words
    // should perhaps use overlap
    //private static (IEnumerable<ContentChunk>, Generator?) PhraseGenerator(ContentChunk chunk) => ([chunk], null);
}
