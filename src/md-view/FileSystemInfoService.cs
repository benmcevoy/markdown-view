namespace MdView
{
    public class FileSystemInfoService
    {
        private readonly string _rootFolder;
        private readonly string[] _allowedExtensions;

        public FolderInfo FileSystem {get; private set;}

        public FileSystemInfoService(string rootFolder, string[] allowedExtensions)
        {
            _rootFolder = rootFolder;
            _allowedExtensions = allowedExtensions;
            
            FileSystem = Build(_rootFolder, new());
        }

        public void Build()
        {
            FileSystem = Build(_rootFolder, new());
        }

        private FolderInfo Build(string path, FolderInfo root)
        {
            // this directory
            root.Name = Path.GetFileName(path);
            root.Path = path;
            root.Uri = ToUri(_rootFolder, path);

            // recurse all directories
            var folders = Directory
                .GetDirectories(path)
                .Select(f => Build(f, new FolderInfo { Parent = root }));

            root.Children.AddRange(folders);

            // add all files
            var files = Directory
                .EnumerateFiles(path)
                .Where(f => _allowedExtensions.Contains(Path.GetExtension(f)))
                .Select(f => new FileInfo
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