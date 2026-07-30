namespace wikd.Routing
{
    public class FileSystemRouter(string rootFolder, string[] allowedExtensions)
    {
        private readonly string _rootFolder = rootFolder;
        private readonly string[] _allowedExtensions = allowedExtensions;

        private FolderRoute? _fileSystem;

        public FolderRoute FileSystem(bool force = false)
        {
            if(force || _fileSystem == null) _fileSystem = Build(_rootFolder, new());

            return _fileSystem;
        }

        private FolderRoute Build(string path, FolderRoute root)
        {
            // this directory
            root.Name = Path.GetFileName(path);
            root.Path = path;
            root.Uri = ToUri(_rootFolder, path);

            // recurse all directories
            var folders = Directory
                .GetDirectories(path)
                .Select(f => Build(f, new FolderRoute { Parent = root }));

            root.Children.AddRange(folders);

            // add all files
            var files = Directory
                .EnumerateFiles(path)
                .Where(f => _allowedExtensions.Contains(Path.GetExtension(f)))
                .Select(f => new FileRoute
                {
                    Parent = root,
                    Name = Path.GetFileName(f),
                    Extension = Path.GetExtension(f),
                    Path = f,
                    Uri = ToUri(_rootFolder, f)
                });

            root.Children.AddRange(files);

            return root;
        }

        private static string ToUri(string root, string path)
        {
            var uri = path[root.Length..];
            return string.IsNullOrWhiteSpace(uri) ? "/" : uri;
        }
    }
}