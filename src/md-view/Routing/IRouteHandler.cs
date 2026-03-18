
namespace MdView.Routing
{
    public interface IRouteHandler : IHandler<string, RouteInfo>
    {
        static string ResolvePath(string root, string relative)
        {
            if (relative.StartsWith('/')) relative = relative[1..];

            return Path.Combine(root, relative);
        }

        static bool IsAllowedExtension(string path, params string[] extensions)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant() ?? "???";

            return extensions.Contains(ext);
        }
    }
}