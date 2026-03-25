namespace MdView
{
    /// <summary>
    /// Router class that maps HTTP request paths to file paths in the filesystem.
    /// </summary>
    /// <remarks>
    /// Creates a new Router instance.
    /// </remarks>
    public class Router(FileSystemInfoService fileSystemService)
    {
        private readonly string _rootPath = fileSystemService.FileSystem.Path;
        private readonly Dictionary<string, FileSystemInfo> _fileSystem = FlattenFileSystem([], fileSystemService.FileSystem);

        /// <summary>
        /// Maps an HTTP request path to a file path in the filesystem.
        /// </summary>
        public FileSystemInfo Map(string requestUrl)
        {
            if (!IsValidRequest(requestUrl)) throw new NotSupportedException("forbidden");

            var path = ResolvePath(_rootPath, requestUrl);

            return _fileSystem.TryGetValue(path, out var fileSystemInfo)
                ? fileSystemInfo
                : throw new NotSupportedException("forbidden");
        }

        private static Dictionary<string, FileSystemInfo> FlattenFileSystem(Dictionary<string, FileSystemInfo> fileSystem, FolderInfo folder)
        {
            fileSystem[folder.Path] = folder;

            foreach (var f in folder.Children)
            {
                if (f is FolderInfo childFolder) fileSystem = FlattenFileSystem(fileSystem, childFolder);
                if (f is FileInfo) fileSystem[f.Path] = f;
            }

            return fileSystem;
        }

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
    }
}
