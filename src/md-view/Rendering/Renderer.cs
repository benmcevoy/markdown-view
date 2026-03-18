using MdView.Routing;

namespace MdView.Rendering
{
    public class Renderer(MarkdownFileRenderer markdownFileRenderer, MainTemplate mainTemplate)
    {
        private readonly IRenderingHandler[] _handlers = [
            new StaticAssetRenderingHandler(),
            new FileRenderingHandler(markdownFileRenderer, mainTemplate),
            new FolderRenderingHandler(markdownFileRenderer, mainTemplate)];

        public ContentInfo Render(RouteInfo route)
        {
            foreach (var r in _handlers)
                if (r.CanHandle(route)) return r.Handle(route);

            return ContentInfo.NotFound();
        }
    }
}



