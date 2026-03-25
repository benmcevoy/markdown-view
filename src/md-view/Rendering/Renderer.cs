using MdView.Templates;

namespace MdView.Rendering
{
    public class Renderer(DefaultTemplate template, Navigation navigation, IRenderingHandler[] handlers)
    {
        private readonly DefaultTemplate _template = template;
        private readonly Navigation _navigation = navigation;
        private readonly IRenderingHandler[] _handlers = handlers;

        public string Render(FileSystemInfo route)
        {
            var main = "nothing to display";

            foreach (var r in _handlers)
            {
                if (r.CanHandle(route))
                {
                    main = r.Handle(route);
                    break;
                }
            }

            // TODO: would be nice if these were handlers too, and were injected
            // annoying to only inject Navigation
            var nav = _navigation.Render(route);
            var title = Title(route, "");
            var breadcrumb = Breadcrumb(route, "", true);

            return _template.Render(title, nav, main, breadcrumb, route.Name);
        }

        private static string Title(FileSystemInfo route, string title)
        {
            title = $"/{route.Name}{title}";

            if (route.Parent != null) title = Title(route.Parent, title);

            return title;
        }

        private static string Breadcrumb(FileSystemInfo route, string title, bool first)
        {
            title = first
                ? $" / {route.Name}"
                : $" / <a href='{route.Uri}'>{route.Name}</a>{title}";

            if (route.Parent != null) title = Breadcrumb(route.Parent, title, false);

            return title;
        }
    }
}
