using System.Data;

namespace MdView.Routing
{
    /// <summary>
    /// Router class that maps HTTP requests to file paths in the filesystem.
    /// </summary>
    /// <remarks>
    /// Creates a new Router instance.
    /// </remarks>
    public class Router(FileSystemRouter fileSystemRouter)
    {
        private const char Space = ' ';
        private const char QuestionMark = '?';
        private const char Hash = '#';
        private const string UnsupportedRequest = "???";
        private static readonly char[] _buffer = new char[2048];
        private readonly string _rootPath = fileSystemRouter.FileSystem().Path;
        private readonly Dictionary<string, Route> _fileSystem = FlattenFileSystem([], fileSystemRouter.FileSystem());

        public Route Map(Stream stream) => Map(Parse(stream));

        private static string Parse(Stream request)
        {
            // GET <request-target>["?"<query>] HTTP/1.1
            // and then headers

            var sr = new StreamReader(request);

            // consume up to the first space
            while (sr.Read() != Space && !sr.EndOfStream) ;

            // unsupported
            if (sr.EndOfStream) return UnsupportedRequest;

            var length = 0;
            var token = (char)sr.Read();

            do
            {
                _buffer[length++] = token;
                token = (char)sr.Read();
            }
            // until a space or ? or #
            while (token != Space && token != QuestionMark && token != Hash);

            return new string(_buffer, 0, length);
        }

        private Route Map(string requestUrl)
        {
            Console.WriteLine(requestUrl);

            if (!IsValidRequest(requestUrl)) return Forbidden();

            var path = ResolvePath(_rootPath, requestUrl);

            return _fileSystem.TryGetValue(path, out var fileSystemInfo)
                ? fileSystemInfo
                : Special(requestUrl);
        }

        private static Route Special(string requestUrl)
        {
            if (requestUrl.Equals(UnsupportedRequest, StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            if (requestUrl.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                return new FileRoute { Extension = ".ico", Name = "favicon.ico" };
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

        private static bool IsValidRequest(string requestUrl)
        {
            // Sanitise
            if (requestUrl == null) return false;

            // path traversal is forbidden, i.e. ./../, throw new NotSupportedException
            if (requestUrl.Contains("./") ||
                requestUrl.Contains("../") ||
                requestUrl.Contains("/.") ||
                requestUrl.Contains(@"\")) return false;

            // query string is forbidden i.e. ?
            // fragment is forbidden i.e. #
            if (requestUrl.Contains('?') || requestUrl.Contains('#')) return false;

            return true;
        }

        private static SpecialRoute NotFound() => new() { Name = "404" };
        private static SpecialRoute Forbidden() => new() { Name = "401" };
    }
}
