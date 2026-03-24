using System.Text.RegularExpressions;
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

        public bool CanHandle(FileSystemInfo input) =>
            input is FileInfo f && (
                f.Extension == ".json" ||
                f.Extension == ".xml" ||
                f.Extension == ".js" ||
                f.Extension == ".cs" ||
                f.Extension == ".ts" ||
                f.Extension == ".html" ||
                f.Extension == ".sh" ||
                f.Extension == ".ps1" 
            );

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