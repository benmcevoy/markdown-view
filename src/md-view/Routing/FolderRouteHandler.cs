namespace MdView.Routing
{
    public class FolderRouteHandler(string rootPath) : IRouteHandler
    {
        private readonly string _rootPath = rootPath;

        public bool CanHandle(string requestPath)
        {
            var path = IRouteHandler.ResolvePath(_rootPath, requestPath);
            return Directory.Exists(path);
        }

        public RouteInfo Handle(string requestPath)
        {
            var path = IRouteHandler.ResolvePath(_rootPath, requestPath);
            return new RouteInfo { RequestPath = requestPath, Path = path, RouteType = RouteType.Folder };
        }
    }
}