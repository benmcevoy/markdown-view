using Microsoft.AspNetCore.Components.Web;

namespace MdView
{
    public class FileSystemInfoService(string rootFolder, string[] allowedExtensions)
    {
        //private readonly bool _includeDotEntries = false;
        private readonly string _rootFolder = rootFolder;
        private readonly string[] _allowedExtensions = allowedExtensions;

        public FolderInfo Build()
        {
            return Build(_rootFolder, new());
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