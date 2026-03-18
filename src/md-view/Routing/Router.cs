namespace MdView.Routing
{
    /// <summary>
    /// Router class that maps HTTP request paths to file paths in the filesystem.
    /// </summary>
    /// <remarks>
    /// Creates a new Router instance.
    /// </remarks>
    public class Router(string rootPath, string staticAssetRootPath)
    {
        private readonly IRouteHandler[] _routerHandlers = [
            new StaticAssetRouteHandler(staticAssetRootPath),
            new FileRouteHandler(rootPath),
            new FolderRouteHandler(rootPath)];

        /// <summary>
        /// Maps an HTTP request path to a file path in the filesystem.
        /// </summary>
        public RouteInfo Map(string requestPath)
        {
            if (!IsValidRequest(requestPath)) throw new NotSupportedException("forbidden");

            foreach (var r in _routerHandlers)
            {
                if (r.CanHandle(requestPath)) return r.Handle(requestPath);
            }

            throw new NotSupportedException("forbidden");
        }

        private static bool IsValidRequest(string requestPath)
        {
            // Sanitise
            if (requestPath == null) return false;

            // path traversal is forbidden, i.e. ./../, throw new NotSupportedException
            if (requestPath.Contains("./") ||
                requestPath.Contains("../") ||
                requestPath.Contains("/.") ||
                requestPath.Contains(@"\")) return false;

            // query string is forbidden i.e. ?
            // fragment is forbidden i.e. #
            if (requestPath.Contains('?') || requestPath.Contains('#')) return false;

            // TODO: uri encoded is forbidden? currently yes
            if (requestPath.Contains('%')) return false;

            return true;
        }
    }
}
