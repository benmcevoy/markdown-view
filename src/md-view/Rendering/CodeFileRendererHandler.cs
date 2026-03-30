using Markdig;
using Markdown.ColorCode;
using FileSystemInfo = MdView.FileSystem.FileSystemInfo;

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

        public string Handle(FileSystemInfo input)
        {
            var file = input as FileSystem.FileInfo;
            var content = File.ReadAllText(file!.Path);
            var markdown = @$"```{file.Extension[1..]}
{content}
```";

            return Markdig.Markdown.ToHtml(markdown, _pipeline);
        }
    }
}