namespace MdView.Rendering
{
    public class FileRenderingHandler(MarkdownFileRenderer fileRenderer) : IRenderingHandler
    {
        private readonly MarkdownFileRenderer _fileRenderer = fileRenderer;

        public bool CanHandle(FileSystemInfo route) => route is FileInfo;

        public string Handle(FileSystemInfo route)=> _fileRenderer.Render(route.Path);
    }
}