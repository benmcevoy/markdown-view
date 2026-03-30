namespace MdView.FileSystem
{
    public class FileSystemInfoService(string rootFolder, string[] allowedExtensions)
    {
        private readonly string _rootFolder = rootFolder;
        private readonly string[] _allowedExtensions = allowedExtensions;

        private FolderInfo? _fileSystem;

        public FolderInfo FileSystem(bool force = false)
        {
            if(force || _fileSystem == null) _fileSystem = Build(_rootFolder, new());

            return _fileSystem;
        }

        public Dictionary<string, FileSystemInfo> FlattenFileSystem(FolderInfo fileSystem) => FlattenFileSystem([], fileSystem);

        private static Dictionary<string, FileSystemInfo> FlattenFileSystem(Dictionary<string, FileSystemInfo> fileSystem, 
                                                                            FolderInfo folder)
        {
            fileSystem[folder.Path] = folder;

            foreach (var f in folder.Children)
            {
                if (f is FolderInfo childFolder) fileSystem = FlattenFileSystem(fileSystem, childFolder);
                if (f is FileInfo) fileSystem[f.Path] = f;
            }

            return fileSystem;
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