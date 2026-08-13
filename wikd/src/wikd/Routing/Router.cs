using wikd.Http;

namespace wikd.Routing
{
    public class Router(Http.Parser parser, FileSystemRouter fileSystemRouter)
    {
        public const string SearchRoute = "__wikd__search";
        public const string AdminRoute = "__wikd__admin";
        private readonly string _rootPath = fileSystemRouter.FileSystem().Path;
        private readonly Dictionary<string, Route> _fileSystem = FlattenFileSystem([], fileSystemRouter.FileSystem());
        private readonly Parser _parser = parser;

        public Route Map(Stream stream) => Map(Parse(stream));

        private Request Parse(Stream request) => _parser.ParseRequest(request);

        private Route Map(Request request)
        {
            if (!IsValidRequest(request)) return new SpecialRoute { Name = "400", StatusCode = HttpStatusCode.ClientError };

            var path = ResolvePath(_rootPath, request.Path);

            return _fileSystem.TryGetValue(path, out var fileSystemInfo)
                ? fileSystemInfo
                : Special(request);
        }

        private static SpecialRoute Special(Request request) =>
             request.Path.ToLowerInvariant() switch
             {
                 SearchRoute => new() { Name = "search", StatusCode = HttpStatusCode.OK, Path = SearchRoute, Query = request.Query },
                 AdminRoute => new() { Name = "admin", StatusCode = HttpStatusCode.OK, Path = AdminRoute },
                 _ => new() { Name = "404", StatusCode = HttpStatusCode.NotFound }
             };

        private static Dictionary<string, Route> FlattenFileSystem(Dictionary<string, Route> fileSystem, FolderRoute folder)
        {
            fileSystem[folder.Path] = folder;

            foreach (var f in folder.Children)
            {
                if (f is FolderRoute childFolder) fileSystem = FlattenFileSystem(fileSystem, childFolder);
                if (f is FileRoute) fileSystem[f.Path] = f;
            }

            return fileSystem;
        }

        // TODO: DRY
        private static string ResolvePath(string root, string relative)
        {
            // make relative
            if (relative.StartsWith('/')) relative = relative[1..];

            relative = Uri.UnescapeDataString(relative);
            relative = relative.Replace('/', Path.DirectorySeparatorChar);

            return Path.Combine(root, relative);
        }

        private static bool IsValidRequest(Request request)
        {
            // Sanitise
            if (request == null) return false;
            if (request.Path == null) return false;

            var requestUrl = request.Path;

            // path traversal is forbidden, i.e. ./../, throw new NotSupportedException
            if (requestUrl.Contains("./") ||
                requestUrl.Contains("../") ||
                requestUrl.Contains("/.") ||
                requestUrl.Contains(@"\")) return false;

            return true;
        }
    }
}
