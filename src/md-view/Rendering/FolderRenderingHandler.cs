using MdView.Routing;

namespace MdView.Rendering
{
    public class FolderRenderingHandler(MarkdownFileRenderer renderer, MainTemplate template) : IRenderingHandler
    {
        private readonly MarkdownFileRenderer _renderer = renderer;
        private readonly MainTemplate _template = template;

        public bool CanHandle(RouteInfo route) => route.RouteType == RouteType.Folder;

        public ContentInfo Handle(RouteInfo route)
        {
            var index = Path.Combine(route.Path, "index.md");

            if (File.Exists(index))
            {
                var title = "TODO";
                var aside = "TODO";
                var main = _renderer.Render(index);

                return new ContentInfo
                {
                    Content = _template.Render(title, aside, main)
                };
            }

            // TODO: generate main instead
            return ContentInfo.NotFound();
        }
    }
}