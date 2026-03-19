using MdView.Navigation;
using MdView.Routing;
using MdView.Templates;

namespace MdView.Rendering
{
    public class FileRenderingHandler(NavigationService navigation, MarkdownFileRenderer fileRenderer, DefaultTemplate template) : IRenderingHandler
    {
        private readonly NavigationService _navigation = navigation;
        private readonly MarkdownFileRenderer _fileRenderer = fileRenderer;
        private readonly DefaultTemplate _template = template;

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