using System.Text;
using HtmlAgilityPack;
using Markdig;

namespace ragd.Service.Clean.Markdown
{
    /// <summary>
    /// Remove markdown formatting characters
    /// </summary>
    public class RemoveFormattingCleaner : ICleaner
    {
        public string Clean(string chunk)
        {
            var result = new StringBuilder(chunk.Length);
            var document = Markdig.Parsers.MarkdownParser.Parse(chunk);
            var html = new HtmlDocument();

            html.LoadHtml(document.ToHtml());

            foreach (var node in html.DocumentNode.SelectNodes("//text()"))
            {
                result.Append(node.InnerText);
            }

            return result.ToString();
        }
    }
}
