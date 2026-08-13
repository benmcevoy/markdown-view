using Markdig.Extensions.Yaml;
using Markdig.Syntax;

namespace ragd.Clean.Markdown
{
    public class RemoveFrontMatterCleaner : ICleaner
    {
        public string Clean(string chunk)
        {
            var document = Markdig.Parsers.MarkdownParser.Parse(chunk);

            foreach (var element in document.Descendants())
            {
                if (element is YamlFrontMatterBlock frontmatter)
                    frontmatter.Remove();
            }

            return document.ToMarkdown();
        }
    }
}
