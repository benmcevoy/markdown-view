using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markdown.ColorCode;
using wikd.Routing;

namespace wikd.Rendering
{
    public class MarkdownFileRenderingHandler : IRenderingHandler
    {
        private readonly MarkdownPipeline _pipeline;

        public MarkdownFileRenderingHandler()
        {
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseYamlFrontMatter()
                .UseColorCode()
                .Build();
        }

        public string[] SupportedFileExtensions => [".md"];

        public string Handle(Route input)
        {
            var content = File.ReadAllText(input.Path);
            var document = Markdig.Markdown.Parse(content, _pipeline);

            document = RewriteImages(input, document);

            return document.ToHtml(_pipeline);
        }

        private static MarkdownDocument RewriteImages(Route input, MarkdownDocument document)
        {
            foreach (var image in document.Descendants<LinkInline>().Where(l => l.IsImage))
            {
                if (string.IsNullOrWhiteSpace(image.Url)) continue;

                var imagePath = ResolvePath(input.Parent!.Path, image.Url);

                if (!File.Exists(imagePath)) continue;

                var ext = Path.GetExtension(imagePath);
                var bytes = File.ReadAllBytes(imagePath);
                var inline = Convert.ToBase64String(bytes);
                var url = $"data:image/{ext[1..]};base64, {inline}";

                image.Url = url;
            }

            return document;
        }

        // TODO: DRY: router.cs
        private static string ResolvePath(string root, string relative)
        {
            // make relative
            if (relative.StartsWith('/')) relative = relative[1..];

            relative = Uri.UnescapeDataString(relative);
            relative = relative.Replace('/', Path.DirectorySeparatorChar);

            return Path.Combine(root, relative);
        }
    }
}