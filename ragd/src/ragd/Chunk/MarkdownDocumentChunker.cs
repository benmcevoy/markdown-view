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

            foreach (var chunk in chunkSpans)
            {
                var span = chunk.Item1;
                var headingPath = chunk.Item2;
                var cleanContent = cleaner.Clean(content.Substring(span.Start, span.Length));

                yield return new ContentChunk(
                    cleanContent,
                    context.SourcePath,
                    headingPath,
                    chunkIndex++,
                    chunkSpans.Count,
                    span.Start,
                    span.End,
                    DateTime.UtcNow);
            }
        }

        private static IEnumerable<(SourceSpan, string[])> Traverse(IEnumerable<MarkdownObject> descendants, int eof)
        {
            var start = descendants.First();
            var headingPath = UpdateHeadingPath(new Stack<(HeadingBlock, string)>(), start as HeadingBlock);

            foreach (var current in descendants.Skip(1))
            {
                var isEOF = current.Span.End == eof;

                if (isEOF)
                {
                    yield return (new SourceSpan(start.Span.Start, current.Span.End), ToPath(headingPath));
                }

                var heading = current.TryGetBlock<HeadingBlock>(out var isHeading);
                var codeFence = current.TryGetBlock<FencedCodeBlock>(out var isCodeFence);

                if (isHeading || isCodeFence)
                {
                    yield return (new SourceSpan(start.Span.Start, current.Span.Start - 1), ToPath(headingPath));

                    start = current;
                    // heading is not null here
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