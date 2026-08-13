namespace ragd.Clean
{
    public class QueryResultCleaner() : ICleaner
    {
        private static readonly ICleaner[] _cleaners =
        [
            new Markdown.RemoveFormattingCleaner(),
            new Text.CondenseWhiteSpaceCleaner(),
        ];

        public string Clean(string chunk) => 
            _cleaners.Aggregate(chunk, (current, cleaner) => cleaner.Clean(current));
    }
}