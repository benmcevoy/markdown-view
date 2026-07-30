using System.Text;
using wikd.Routing;

namespace wikd.Rendering
{
    public class FolderRenderingHandler() : IRenderingHandler
    {
        public string[] SupportedFileExtensions => [];

        public bool CanHandle(Route route) => route is FolderRoute;

        public string Handle(Route route)
        {
            var sb = new StringBuilder();
            var folder = route as FolderRoute;

            if (folder!.Children.Count == 0) return "Nothing here...";

            foreach (var f in folder.OrderedChildren())
            {
                sb.AppendLine($"<div class='{f.RouteType()}'><a href='{f.Uri}'> {f.Name}</a></div>");
            }

            return sb.ToString();
        }
    }
}