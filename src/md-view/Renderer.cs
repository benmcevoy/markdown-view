using System.Text.RegularExpressions;

namespace MdView
{
    /// <summary>
    /// Router class that maps HTTP request paths to file paths in the filesystem.
    /// </summary>
    public class Renderer
    {
        private readonly string _mainTemplate;

        public Renderer(string wwwroot = "wwwroot")
        {
            _mainTemplate = File.ReadAllText(Path.Combine(wwwroot, "html/main.html"));
        }

        public async Task<ContentInfo> Render(RouteInfo route)
        {
            if (route.IsStaticAsset)
            {
                return await RenderStaticAsset(route);
            }

            if (route.IsFolder)
            {
                return await RenderFolder(route);
            }

            var title = "TODO";
            var aside = "TODO";
            var main = await RenderMarkdown(route.Path);

            return new ContentInfo
            {
                Content = MainTemplate(title, aside, main)
            };
        }

        private async Task<ContentInfo> RenderFolder(RouteInfo route)
        {
            var index = Path.Combine(route.Path, "index.md");

            if (File.Exists(index))
            {
                var title = "TODO";
                var aside = "TODO";
                var main = await RenderMarkdown(index);

                return new ContentInfo
                {
                    Content = MainTemplate(title, aside, main)
                };
            }

            return ContentInfo.NotFound();
        }

        private static async Task<ContentInfo> RenderStaticAsset(RouteInfo route)
        {
            if (File.Exists(route.Path))
            {
                var extension = Path.GetExtension(route.Path);
                var contentType = "text/html"; // Default

                if (extension == ".css") contentType = "text/css";
                if (extension == ".js") contentType = "text/javascript";

                return new ContentInfo
                {
                    Content = await File.ReadAllTextAsync(route.Path),
                    ContentType = contentType
                };
            }

            return ContentInfo.NotFound();
        }

        private static async Task<string> RenderMarkdown(string filePath)
        {
            var content = await File.ReadAllTextAsync(filePath);
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

        private string MainTemplate(string title, string aside, string main)
        {
            var template = _mainTemplate;

            template = template.Replace("{{title}}", title);
            template = template.Replace("{{aside}}", aside);
            template = template.Replace("{{markdown}}", main);

            return template;
        }
    }

    public class ContentInfo
    {
        public string Content { get; set; } = "";
        public string ContentType { get; set; } = "text/html";
        public int StatusCode { get; set; } = 200;
        public static ContentInfo NotFound() => new() { StatusCode = 404, Content = "text/plain" };
    }
}



