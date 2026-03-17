using System;
using System.IO;
using System.Text.Encodings.Web;

namespace MdView
{
    /// <summary>
    /// Router class that maps HTTP request paths to file paths in the filesystem.
    /// </summary>
    /// <remarks>
    /// Creates a new Router instance.
    /// </remarks>
    /// <param name="rootPath">The root directory path to scan for files</param>
    public class Router(string rootPath)
    {
        private readonly string _rootPath = rootPath;

        /// <summary>
        /// Maps an HTTP request path to a file path in the filesystem.
        /// </summary>
        public RouteInfo Map(string requestPath)
        {
            if (!IsValidRequest(requestPath)) throw new NotSupportedException("forbidden");

            if (IsStaticRoute(requestPath))
            {
                var path = ResolvePath("wwwroot", requestPath);

                return IsAllowedExtension(path, ".css", ".js")
                    ? new RouteInfo { RequestPath = requestPath, Path = path, IsStaticAsset = true }
                    : throw new NotSupportedException("forbidden");
            }

            var result = new RouteInfo
            {
                RequestPath = requestPath,
                Path = ResolvePath(_rootPath, requestPath),
            };

            result.IsFolder = !File.Exists(result.Path);

            return result;
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
            if (requestPath.Contains('%') ) return false;

            return true;
        }

        private string ResolvePath(string root, string relative)
        {
            if(relative.StartsWith("/")) relative = relative.Substring(1);

            return Path.Combine(root, relative);
        }

        private static bool IsAllowedExtension(string path, params string[] extensions)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant() ?? "???";

            return extensions.Contains(ext);
        }

        private static bool IsStaticRoute(string requestPath)
        {
            return requestPath.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
              requestPath.StartsWith("/js/", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class RouteInfo
    {
        public string RequestPath { get; set; } = "";
        public string Path { get; set; } = "";
        public bool IsStaticAsset { get; set; } = false;
        public bool IsFolder { get; set; } = false;
    }
}
