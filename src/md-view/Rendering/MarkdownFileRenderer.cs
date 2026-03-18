using System.Text.RegularExpressions;

namespace MdView.Rendering
{
    public class MarkdownFileRenderer
    {
        public string Render(string filePath)
        {
            var content = File.ReadAllText(filePath);
            var markdownWithLinks = ResolveMarkdownLink(content);

            return Markdig.Markdown.ToHtml(markdownWithLinks);
        }

        private static string ResolveMarkdownLink(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return markdown;
            }

            // Pattern to match markdown links: [text](path)
            var linkPattern = new Regex(@"\[([^\]]+)\]\(([^)]+)\)", RegexOptions.Compiled);

            var result = linkPattern.Replace(markdown, (match) =>
            {
                var linkPath = match.Groups[2].Value;

                // Handle relative paths
                if (linkPath.StartsWith("./"))
                {
                    linkPath = linkPath.Substring(2);
                }
                else if (linkPath.StartsWith("../"))
                {
                    // Skip for now - don't resolve parent directory links
                    return match.Value;
                }
                else if (linkPath.StartsWith("#"))
                {
                    // Anchor link - keep as is
                    return match.Value;
                }
                else
                {
                    // Absolute path or external link - keep as is
                    return match.Value;
                }

                return match.Value;
            });

            return result;
        }
    }
}