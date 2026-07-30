namespace ragd.Service.Clean
{
    public class MarkdownChunkCleaner() : ICleaner
    {
        private static readonly Markdown.CodeBlockCleaner _codeBlockCleaner = new();

        private static readonly ICleaner[] _cleaners =
        [
            new Html.RemoveHtmlHrVariantsCleaner(),
            new Markdown.RemoveThematicBreakBlocksCleaner(),
            new Html.ReplaceUrlWithAnchorTextCleaner(),
          // TODO: new Markdown.RemoveFormattingCleaner(),
            new Markdown.RemoveFrontMatterCleaner(),
          // TODO:  new Markdown.ConvertTableToCsvCleaner(),
            new Text.CondenseWhiteSpaceCleaner(),
        ];

        public string Clean(string chunk)
        {
            if (IsCodeFence(chunk)) return _codeBlockCleaner.Clean(chunk);

            return _cleaners.Aggregate(chunk, (current, cleaner) => cleaner.Clean(current));
        }

        private static bool IsCodeFence(string chunk) =>
            chunk.StartsWith("```") || chunk.StartsWith("~~~");
    }
}