using Markdig.Syntax;

namespace ragd.Service.Clean.Markdown
{
    public class RemoveThematicBreakBlocksCleaner : ICleaner
    {
        public string Clean(string chunk)
        {
            var document = Markdig.Parsers.MarkdownParser.Parse(chunk);

            foreach (var element in document.Descendants())
            {
                if (element is ThematicBreakBlock line)
                    line.Remove();
            }

            return document.ToMarkdown();
        }
    }
}
