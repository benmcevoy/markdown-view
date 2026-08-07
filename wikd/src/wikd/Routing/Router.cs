using wikd.Http;

namespace wikd.Routing
{
    public class Router(Http.Parser parser, FileSystemRouter fileSystemRouter)
    {
        private const string SearchRoute = "__ragd__search";
        private const string AdminRoute = "__ragd__admin";
        private const string UnsupportedRequest = "???";
        private readonly string _rootPath = fileSystemRouter.FileSystem().Path;
        private readonly Dictionary<string, Route> _fileSystem = FlattenFileSystem([], fileSystemRouter.FileSystem());
        private readonly Parser _parser = parser;

        public Route Map(Stream stream) => Map(Parse(stream));

        private Request Parse(Stream request) => _parser.ParseRequest(request);

        private Route Map(Request request)
        {
            if (!IsValidRequest(request)) return Forbidden();

            var path = ResolvePath(_rootPath, request.Path);

            return _fileSystem.TryGetValue(path, out var fileSystemInfo)
                ? fileSystemInfo
                : Special(request);
        }

        private static Route Special(Request request)
        {
            if (request.Path.Equals(SearchRoute, StringComparison.OrdinalIgnoreCase))
            {
                return new SpecialRoute { Name = "search", StatusCode = HttpStatusCode.OK };
            }

            if (request.Path.Equals(AdminRoute, StringComparison.OrdinalIgnoreCase))
            {
                return new SpecialRoute { Name = "admin", StatusCode = HttpStatusCode.OK };
            }

            if (request.Path.Equals(UnsupportedRequest, StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            return NotFound();
        }

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

        private static SpecialRoute NotFound() => new() { Name = "404", StatusCode = HttpStatusCode.NotFound };
        private static SpecialRoute Forbidden() => new() { Name = "401", StatusCode = HttpStatusCode.Forbidden };
    }
}
