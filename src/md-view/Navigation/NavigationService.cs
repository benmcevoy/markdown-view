namespace MdView.Navigation
{
    public class NavigationService(string rootFolder, string[] allowedExtensions)
    {
        private const string FolderExtension = "FOLDER";
        //private readonly bool _includeDotEntries = false;
        private readonly string _rootFolder = rootFolder;
        private readonly string[] _allowedExtensions = allowedExtensions.Select(x => "*" + x).ToArray();

        // build navigation, start at root and recurse, look allowed extension
        // use file/folder names only, include the ext.
        // for folder use FOLDER as extension

        public NavigationItem Build()
        {
            return Build(_rootFolder, new NavigationItem());
        }

        private NavigationItem Build(string path, NavigationItem root)
        {
            // this directory
            root.Name = Path.GetFileName(path);
            root.RelativeUrl = MakeRelative(path);
            root.Extension = FolderExtension;
           
            // recurse all directories
            var folders = Directory
                .GetDirectories(path)
                .Select(f => Build(f, new NavigationItem()));

            root.Children.AddRange(folders);

            // add all files
            var files = Directory
                .GetFiles(path, string.Join(',', _allowedExtensions))
                .Select(f => new NavigationItem
                {
                    Name = Path.GetFileName(f),
                    Extension = Path.GetExtension(f),
                    RelativeUrl = MakeRelative(f)
                });

            root.Children.AddRange(files);

            return root;
        }

        private string MakeRelative(string path) => path[_rootFolder.Length..];
    }
}