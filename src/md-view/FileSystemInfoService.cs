namespace MdView
{
    public class FileSystemInfoService(string rootFolder, string[] allowedExtensions)
    {
        //private readonly bool _includeDotEntries = false;
        private readonly string _rootFolder = rootFolder;
        private readonly string[] _allowedExtensions = [.. allowedExtensions.Select(x => "*" + x)];

        public FolderInfo Build()
        {
            return Build(_rootFolder, new FolderInfo());
        }

        private FolderInfo Build(string path, FolderInfo root)
        {
            // this directory
            root.Name = Path.GetFileName(path);
            root.Path = path;
           
            // recurse all directories
            var folders = Directory
                .GetDirectories(path)
                .Select(f => Build(f, new FolderInfo()));

            root.Children.AddRange(folders);

            // add all files
            var files = Directory
                .GetFiles(path, string.Join(',', _allowedExtensions))
                .Select(f => new FileInfo
                {
                    Name = Path.GetFileName(f),
                    Extension = Path.GetExtension(f),
                    Path = f
                });

            root.Children.AddRange(files);

            return root;
        }
    }
}