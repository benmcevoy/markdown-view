using System.Text;
using wikd.Http;
using wikd.Routing;
using wikd.Templates;

namespace wikd.Rendering
{
    public class Renderer(DefaultTemplate template, IRenderingHandler[] handlers)
    {
        private readonly DefaultTemplate _template = template;
        private readonly IRenderingHandler[] _handlers = handlers;

        public ContentInfo Render(Route route)
        {
            var main = "nothing to see here";

            foreach (var r in _handlers)
            {
                if (!r.CanHandle(route)) continue;
                main = r.Handle(route);
                break;
            }

            var nav = Navigation(route);
            var title = Title(route, "");
            var breadcrumb = Breadcrumb(route, "", true);

            return new()
            {
                Content = _template.Render(title, nav, main, breadcrumb, route.Name),
                StatusCode = route is SpecialRoute s ? s.StatusCode : HttpStatusCode.OK
            };
        }

        private static string Title(Route route, string title)
        {
            title = $"/{route.Name}{title}";

            if (route.Parent != null) title = Title(route.Parent, title);

            return title;
        }

        private static string Breadcrumb(Route route, string title, bool first)
        {
            title = first
                ? $" / {route.Name}"
                : $" / <a href='{route.Uri}'>{route.Name}</a>{title}";

            if (route.Parent != null) title = Breadcrumb(route.Parent, title, false);

            return title;
        }

        private static string Navigation(Route current)
        {
            var root = current.Parent ?? current as FolderRoute;

            if (root == null) return "";

            while (root?.Parent != null) root = root.Parent;

            return @$"
<nav>
    {Navigation(root!, current)}
</nav>";
        }

        private static string Navigation(FolderRoute root, Route current)
        {
            var sb = new StringBuilder("<ul>");

            foreach (var x in root.OrderedChildren())
            {
                var isFolder = x is FolderRoute;

                sb.AppendLine(@$"<li class='{x.RouteType()}'>");

                if (isFolder)
                {
                    sb.AppendLine(@$"<details {CurrentFolderOpen(x, current)}>
    <summary>
        <a class='{CurrentClass(x, current)}' href='{x.Uri}'>{x.Name}</a>
    </summary>");
                }
                else
                {
                    sb.AppendLine(@$"<a class='{CurrentClass(x, current)}' href='{x.Uri}'>{x.Name}</a>");
                }

                if (x is FolderRoute f && f.Children.Count > 0)
                {
                    sb.AppendLine(Navigation(f, current));
                }

                if (isFolder) sb.AppendLine("</details>");

                sb.AppendLine("</li>");
            }

            sb.Append("</ul>");

            return sb.ToString();
        }
        private static string CurrentFolderOpen(Route candidate, Route current) =>
            current.Path.StartsWith(candidate.Path, StringComparison.Ordinal) ? "open" : "";

        private static string CurrentClass(Route candidate, Route current) =>
             candidate.Path.Equals(current.Path, StringComparison.Ordinal) ? "current" : "";
    }
}
