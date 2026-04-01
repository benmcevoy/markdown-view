using System.Text;
using MdView.Routing;
using MdView.Templates;

namespace MdView.Rendering
{
    public class Renderer(DefaultTemplate template, IRenderingHandler[] handlers)
    {
        private readonly DefaultTemplate _template = template;
        private readonly IRenderingHandler[] _handlers = handlers;

        public ContentInfo Render(Route route)
        {
            // TODO: this is nonsense
            if (route is SpecialRoute s)
            {
                if(s.Name == "favicon.ico") return new ContentInfo { Content = 
                @"<link rel='icon'
        href='data:image/x-icon;base64,AAABAAEAEBAQAAAAAAAoAQAAFgAAACgAAAAQAAAAIAAAAAEABAAAAAAAgAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAZlX/ADOA/wAzVf8AfwB/ADOq/wD//wAA/wD/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAZmYAAGZmAAAAEQAAEQAAAAA1AAAzAAAAABFmZiMAAAAAAHd3AAAAAAAAZmYAAAAAZgB3dwBmAAB3AGZmAHcAAAB3d3d3AAAAAAB3dwAAAAAAd3d3dwAAAHcAd3cAdwAAdwB3dwB3AAAAd0REdwAAAABEAABEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA'
        type='image/x-icon' />",
         ContentType = "image/x-icon" };

                return new ContentInfo { Content = "404 - Not Found", StatusCode = "404 NotFound" };
            }

            var main = "nothing to display";

            foreach (var r in _handlers)
            {
                if (r.CanHandle(route))
                {
                    main = r.Handle(route);
                    break;
                }
            }

            var nav = Navigation(route);
            var title = Title(route, "");
            var breadcrumb = Breadcrumb(route, "", true);

            return new() { Content = _template.Render(title, nav, main, breadcrumb, route.Name) };
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

            if(root == null) return "";

            while(root?.Parent != null) root = root.Parent;

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
                sb.AppendLine(@$"<li class='{x.RouteType()}'>
                                    <a class='{CurrentClass(x, current)}' href='{x.Uri}'>{x.Name}</a>");

                if (x is FolderRoute f && f.Children.Count > 0)
                {
                    sb.AppendLine(Navigation(f, current));
                }

                sb.AppendLine("</li>");
            }

            sb.Append("</ul>");

            return sb.ToString();
        }

        private static string CurrentClass(Route candidate, Route current) =>
             candidate.Path.Equals(current.Path, StringComparison.Ordinal) ? "current" : "";
    }
}
