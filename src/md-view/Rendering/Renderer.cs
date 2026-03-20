using MdView.Navigation;
using MdView.Routing;
using MdView.Templates;

namespace MdView.Rendering
{
    public class Renderer(NavigationService navigation, MarkdownFileRenderer markdownFileRenderer, DefaultTemplate template)
    {
        private readonly IRenderingHandler[] _handlers = [
            new FileRenderingHandler(navigation, markdownFileRenderer, template),
            new FolderRenderingHandler(navigation, markdownFileRenderer, template)];

        public ContentInfo Render(RouteInfo route)
        {
            foreach (var r in _handlers)
                if (r.CanHandle(route)) return r.Handle(route);

            return ContentInfo.NotFound();
        }
    }
}



