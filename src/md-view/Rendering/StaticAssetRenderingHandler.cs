using MdView.Routing;

namespace MdView.Rendering
{
    public class StaticAssetRenderingHandler : IRenderingHandler
    {
        public bool CanHandle(RouteInfo route) => route.RouteType == RouteType.Static;

        public ContentInfo Handle(RouteInfo route)
        {
            if (!File.Exists(route.Path)) return ContentInfo.NotFound();

            var extension = Path.GetExtension(route.Path);
            var contentType = "text/html"; // Default

            if (extension == ".css") contentType = "text/css";
            if (extension == ".js") contentType = "text/javascript";

            return new ContentInfo
            {
                Content = File.ReadAllText(route.Path),
                ContentType = contentType
            };
        }
    }
}