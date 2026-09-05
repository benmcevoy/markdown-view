using http;
using wikd.Rendering;

namespace wikd.Routing;

public class Router
{
    public const string SearchRoute = "__wikd__search";
    public const string AdminRoute = "__wikd__admin";
    private readonly string _rootPath;
    private readonly Dictionary<string, Route> _fileSystem;
    private readonly Renderer _renderer;

    public Router(FileSystemRouter fileSystemRouter, Renderer renderer)
    {
        _rootPath = fileSystemRouter.FileSystem().Path;
        _fileSystem = FlattenFileSystem([], fileSystemRouter.FileSystem());
        _renderer = renderer;
    }

    public Response RequestReceived(Request request) => _renderer.Render(Map(request));

    private Route Map(Request request)
    {
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
}