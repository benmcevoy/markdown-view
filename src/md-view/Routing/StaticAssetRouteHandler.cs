namespace MdView.Routing
{
    public class StaticAssetRouteHandler(string rootPath) : IRouteHandler
    {
        private readonly string _rootPath = rootPath;
        private readonly static string[] _allowedRoutes = { "/css/", "/js/" };
        private readonly static string[] _allowFileExtensions = { ".css", ".js" };

        public bool CanHandle(string requestPath)
        {
            foreach (var r in _allowedRoutes)
            {
                if (requestPath.StartsWith(r, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        public RouteInfo Handle(string requestPath)
        {
            var path = IRouteHandler.ResolvePath(_rootPath, requestPath);

            return IRouteHandler.IsAllowedExtension(path, _allowFileExtensions)
                ? new RouteInfo { RequestPath = requestPath, Path = path, RouteType = RouteType.Static }
                : throw new NotSupportedException("forbidden");
        }
    }
}