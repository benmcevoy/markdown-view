namespace MdView
{
    /// <summary>
    /// Router class that maps HTTP request paths to file paths in the filesystem.
    /// </summary>
    /// <remarks>
    /// Creates a new Router instance.
    /// </remarks>
    public class Router(FolderInfo rootFolder)
    {
        private readonly string _rootPath = rootFolder.Path;
        private readonly Dictionary<string, FileSystemInfo> _fileSystem = FlattenFileSystem([], rootFolder);

        /// <summary>
        /// Maps an HTTP request path to a file path in the filesystem.
        /// </summary>
        public FileSystemInfo Map(string requestPath)
        {
            if (!IsValidRequest(requestPath)) throw new NotSupportedException("forbidden");

            var path = ResolvePath(_rootPath, requestPath);

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

            return Path.Combine(root, relative);
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

            return true;
        }
    }
}
