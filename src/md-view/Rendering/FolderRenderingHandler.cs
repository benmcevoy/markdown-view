using MdView.Navigation;
using MdView.Routing;
using MdView.Templates;

namespace MdView.Rendering
{
    public class FolderRenderingHandler(NavigationService navigation, MarkdownFileRenderer renderer, DefaultTemplate template) : IRenderingHandler
    {
        private readonly NavigationService _navigation = navigation;
        private readonly MarkdownFileRenderer _renderer = renderer;
        private readonly DefaultTemplate _template = template;

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