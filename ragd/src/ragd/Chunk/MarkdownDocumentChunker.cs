using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using ragd.Clean;

namespace ragd.Chunk
{
    public class MarkdownDocumentChunker(MarkdownChunkCleaner cleaner) : IDocumentChunker
    {
        private static readonly NormalizeOptions _normalizationOptions = new()
        {
            EmptyLineAfterCodeBlock = false,
            EmptyLineAfterHeading = false,
            EmptyLineAfterThematicBreak = false,
        };

        public bool CanHandle(Document context) => context.Extension.Equals(".md", StringComparison.OrdinalIgnoreCase);

        public IEnumerable<ContentChunk> Handle(Document context)
        {
            var content = context.Content;
            var document = Markdig.Parsers.MarkdownParser.Parse(content);

            // Descendants flattens depth first, which is what we want
            var dfs = document.Descendants();
            var chunkSpans = Traverse(dfs, content.Length - 1).ToList();
            var chunkIndex = 0;

            foreach ((var span, var headingPath, var chunkType) in chunkSpans)
            {
                var cleanContent = cleaner.Clean(content.Substring(span.Start, span.Length));

                yield return new ContentChunk(
                    cleanContent,
                    context.SourcePath,
                    headingPath,
                    chunkIndex++,
                    chunkSpans.Count,
                    span.Start,
                    span.End,
                    DateTime.UtcNow,
                    chunkType);
            }
        }

        private static IEnumerable<(SourceSpan, string[], ChunkType)> Traverse(IEnumerable<MarkdownObject> descendants, int eof)
        {
            var start = descendants.First();
            var headingPath = UpdateHeadingPath(new Stack<(HeadingBlock, string)>(), start as HeadingBlock);

            // TODO: why Skip? what if First was not a heading?
            // perhaps use TryUpdateHeadingPath or something instead
            foreach (var current in descendants.Skip(1))
            {
                var isEOF = current.Span.End == eof;

                if (isEOF)
                {
                    yield return (
                        new SourceSpan(start.Span.Start, current.Span.End), 
                        ToPath(headingPath), 
                        ChunkType.Markdown);
                    break;
                }

                if (current.TryGetBlockAs<FencedCodeBlock>(out var codeFence))
                {
                    yield return (
                        new SourceSpan(start.Span.Start, current.Span.Start - 1), 
                        ToPath(headingPath), 
                        ChunkType.Code);

                    start = current;
                    continue;
                }


                if (current.TryGetBlockAs<HeadingBlock>(out var heading))
                {
                    yield return (new SourceSpan(start.Span.Start, current.Span.Start - 1), ToPath(headingPath), ChunkType.Markdown);

                    start = current;

                    // heading is not null here
                    // TODO: how do you know heading is not null?
                    headingPath = UpdateHeadingPath(headingPath, heading!);
                }
            }
        }

        private static string[] ToPath(Stack<(HeadingBlock, string)> headingPath)
            => [.. headingPath.Reverse().Select(x => x.Item2)];

        private static Stack<(HeadingBlock, string)> UpdateHeadingPath(Stack<(HeadingBlock, string)> headingPath, HeadingBlock? heading)
        {
            if (heading == null) return headingPath;

            var previous = headingPath.Count == 0 ? heading : headingPath.Peek().Item1;

            while (!(headingPath.Count == 0) && heading.Level <= previous.Level)
            {
                previous = headingPath.Pop().Item1;
            }

            headingPath.Push((heading, heading.ToMarkdown(_normalizationOptions).Replace("#", string.Empty).Trim()));

            return headingPath;
        }
    }
}