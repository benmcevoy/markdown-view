using Markdig;
using Markdown.ColorCode;

namespace MdView.Rendering
{
    public class MarkdownFileRendererHandler : IRenderingHandler
    {
        private readonly MarkdownPipeline _pipeline;

        public MarkdownFileRendererHandler()
        {
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseYamlFrontMatter()
                .UseColorCode()
                .Build();
        }

        public string[] SupportedFileExtensions => [".md"];

        public bool CanHandle(FileSystemInfo input) => 
            input is FileInfo f && SupportedFileExtensions.Contains(f.Extension);

        public string Handle(FileSystemInfo input)
        {
            var content = File.ReadAllText(input.Path);
            var document = Markdig.Markdown.Parse(content, _pipeline);

            return document.ToHtml(_pipeline);
        }
    }
}