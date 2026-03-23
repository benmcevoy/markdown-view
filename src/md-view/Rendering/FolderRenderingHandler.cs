namespace MdView.Rendering
{
    public class FolderRenderingHandler(MarkdownFileRenderer renderer) : IRenderingHandler
    {
        private readonly MarkdownFileRenderer _renderer = renderer;

        public bool CanHandle(FileSystemInfo route) => route is FolderInfo;

        public string Handle(FileSystemInfo route)
        {
            var index = Path.Combine(route.Path, "index.md");

            return File.Exists(index) 
                ? _renderer.Render(index) 
                : GenerateFolderContent(route);
        }

        private static string GenerateFolderContent(FileSystemInfo route)
        {
            return "<h1> TODO: generate a folder listing page/h1>";
        }
    }
}