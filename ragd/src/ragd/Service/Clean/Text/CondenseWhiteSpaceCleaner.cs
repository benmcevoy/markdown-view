using System.Text.RegularExpressions;

namespace ragd.Service.Clean.Text
{
    public partial class CondenseWhiteSpaceCleaner : ICleaner
    {
        // condense sequences of whitespace longer than two units
        [GeneratedRegex(@"[^\S\r\n]{2,}")] private static partial Regex _whitespaceExceptNewlinesPattern();
                
        // condense sequences of new line longer than two units
        [GeneratedRegex(@"\n{2,}")] private static partial Regex _multipleLinesPattern();

        public string Clean(string chunk)
        {
            // normalize line endings
            chunk = chunk.Replace("\r\n", "\n");

            chunk = _whitespaceExceptNewlinesPattern().Replace(chunk, " ");
            chunk = chunk.Replace("\n ", "\n");
            chunk = _multipleLinesPattern().Replace(chunk, "\n\n");
            chunk = chunk.Trim();

            return chunk;
        }
    }
}
