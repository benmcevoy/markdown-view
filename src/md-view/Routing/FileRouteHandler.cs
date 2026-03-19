namespace MdView.Routing
{
    public class FileRouteHandler(string rootPath, string[] allowedFileExtensions) : IRouteHandler
    {
        private readonly string _rootPath = rootPath;
        private readonly string[] _allowedFileExtensions = allowedFileExtensions;

        public bool CanHandle(string requestPath)
        {
            var path = IRouteHandler.ResolvePath(_rootPath, requestPath);
            return File.Exists(path) && IRouteHandler.IsAllowedExtension(path, _allowedFileExtensions);
        }

        public RouteInfo Handle(string requestPath)
        {
            var path = IRouteHandler.ResolvePath(_rootPath, requestPath);
            return new RouteInfo { RequestPath = requestPath, Path = path, RouteType = RouteType.File };
        }
    }
}