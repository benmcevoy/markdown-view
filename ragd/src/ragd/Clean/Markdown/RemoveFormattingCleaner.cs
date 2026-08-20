using System.Text;
using HtmlAgilityPack;
using Markdig;

namespace ragd.Clean.Markdown
{
    /// <summary>
    /// Remove markdown formatting characters
    /// </summary>
    public class RemoveFormattingCleaner : ICleaner
    {
        public string Clean(string chunk)
        {
            var document = Markdig.Parsers.MarkdownParser.Parse(chunk);
            var html = new HtmlDocument();

            html.LoadHtml(document.ToHtml());

            var nodes = html.DocumentNode.SelectNodes("//text()");

            if(nodes is null) return html.DocumentNode.InnerText;

            var result = new StringBuilder(chunk.Length);

            foreach (var node in nodes)
            {
                result.Append(node.InnerText);
            }

            return result.ToString();
        }
    }
}
