using Markdig;
using Markdown.ColorCode;
using wikd.Routing;

namespace wikd.Rendering
{
    public class CodeFileRenderingHandler : IRenderingHandler
    {
        private readonly MarkdownPipeline _pipeline;

        public CodeFileRenderingHandler()
        {
            _pipeline = new MarkdownPipelineBuilder()
                .UseColorCode()
                .Build();
        }

        public string[] SupportedFileExtensions => [".json", ".xml", ".js", ".cs", ".ts", ".html", ".sh", ".ps1", ".css", ".sql"];

        public async Task<string> HandleAsync(Route input)
        {
            var file = input as FileRoute;
            var content = await File.ReadAllTextAsync(file!.Path);
            var markdown = @$"```{file.Extension[1..]}
{content}
```";

            return Markdig.Markdown.ToHtml(markdown, _pipeline);
        }
    }
}