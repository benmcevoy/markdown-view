using Markdig;
using Markdown.ColorCode;

namespace MdView.Rendering
{
    public class CodeFileRendererHandler : IRenderingHandler
    {
        private readonly MarkdownPipeline _pipeline;

        public CodeFileRendererHandler()
        {
            _pipeline = new MarkdownPipelineBuilder()
                .UseColorCode()
                .Build();
        }

        public string[] SupportedFileExtensions => [".json", ".xml", ".js", ".cs", ".ts", ".html", ".sh", ".ps1"];

        public bool CanHandle(FileSystemInfo input) =>
            input is FileInfo f && SupportedFileExtensions.Contains(f.Extension);

        public string Handle(FileSystemInfo input)
        {
            var file = input as FileInfo;
            var content = File.ReadAllText(file!.Path);
            var markdown = @$"```{file.Extension[1..]}
{content}
```";

            return Markdig.Markdown.ToHtml(markdown, _pipeline);
        }
    }
}