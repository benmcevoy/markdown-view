using System.Text;
using wikd.Routing;

namespace wikd.Rendering
{
    public class FolderRenderingHandler() : IRenderingHandler
    {
        public string[] SupportedFileExtensions => [];

        public bool CanHandle(Route route) => route is FolderRoute;

        public Task<string> HandleAsync(Route route)
        {
            var sb = new StringBuilder();
            var folder = route as FolderRoute;

            if (folder!.Children.Count == 0) return Task.FromResult("Nothing here...");

            foreach (var f in folder.OrderedChildren())
            {
                sb.AppendLine($"<div class='{f.RouteType()}'><a href='{f.Uri}'> {f.Name}</a></div>");
            }

            return Task.FromResult(sb.ToString());
        }
    }
}