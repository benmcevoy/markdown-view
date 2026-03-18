using MdView.Routing;

namespace MdView.Rendering
{
    public class FileRenderingHandler(MarkdownFileRenderer fileRenderer, MainTemplate template) : IRenderingHandler
    {
        private readonly MarkdownFileRenderer _fileRenderer = fileRenderer;
        private readonly MainTemplate _template = template;

        public bool CanHandle(RouteInfo route) => route.RouteType == RouteType.File;

        public ContentInfo Handle(RouteInfo route)
        {
            var title = "TODO";
            var aside = "TODO";
            var main = _fileRenderer.Render(route.Path);

            return new ContentInfo
            {
                Content = _template.Render(title, aside, main)
            };
        }
    }
}