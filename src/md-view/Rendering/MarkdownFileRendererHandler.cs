using System.Text.RegularExpressions;
using Markdig;
using Markdown.ColorCode;

namespace MdView.Rendering
{
    public class MarkdownFileRendererHandler : IRenderingHandler
    {
        private const string DefaultFile = "index.md";

        private readonly MarkdownPipeline _pipeline;

        public MarkdownFileRendererHandler()
        {
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseColorCode()
                .Build();
        }

        public bool CanHandle(FileSystemInfo input) =>
             (input is FileInfo f && f.Extension == ".md") || 
             (input is FolderInfo folder && folder.Children.Any(x => x.Name == DefaultFile));

        public string Handle(FileSystemInfo input)
        {
            var path = input.Path;
            if(input is FolderInfo) path = Path.Combine(input.Path, DefaultFile);

            var content = File.ReadAllText(path);
            var markdownWithLinks = ResolveMarkdownLink(content);

            return Markdig.Markdown.ToHtml(markdownWithLinks, _pipeline);
        }

        // TODO: this needs tests as Qwen3.5 wrote it and it is probably whack
        // should handle:
        // link.md
        // ./link.md
        // ../folder/link.md
        // and images which are converted to data:
        // and if cannot resolve replace with some broken link markup message
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