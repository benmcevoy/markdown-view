namespace MdView.Routing
{
    public class FileRouteHandler(string rootPath) : IRouteHandler
    {
        private readonly string _rootPath = rootPath;
        private readonly static string[] _allowFileExtensions = [".md"];

        public bool CanHandle(string requestPath)
        {
            var path = IRouteHandler.ResolvePath(_rootPath, requestPath);
            return File.Exists(path) && IRouteHandler.IsAllowedExtension(path, _allowFileExtensions);
        }

        public RouteInfo Handle(string requestPath)
        {
            var path = IRouteHandler.ResolvePath(_rootPath, requestPath);
            return new RouteInfo { RequestPath = requestPath, Path = path, RouteType = RouteType.File };
        }
    }
}