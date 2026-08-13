using Markdig.Extensions.Tables;
using Markdig.Syntax;

namespace ragd.Clean.Markdown
{
    public class ConvertTableToCsvCleaner : ICleaner
    {
        public string Clean(string chunk)
        {
            var document = Markdig.Parsers.MarkdownParser.Parse(chunk);

            foreach (var element in document.Descendants())
            {
                // TODO: this does not work
                if (element is Table table)
                {
                    table.ReplaceBy(ToCsv(table));
                }
            }

            return document.ToMarkdown();
        }

        private static Block ToCsv(Table table) => throw new NotImplementedException();
    }
}
