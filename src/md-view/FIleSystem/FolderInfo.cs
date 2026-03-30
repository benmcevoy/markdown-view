namespace MdView.FileSystem
{
    public class FolderInfo : FileSystemInfo
    {
        public List<FileSystemInfo> Children { get; set; } = [];

        public IEnumerable<FileSystemInfo> OrderedChildren()
        {
            var folders = new List<FileSystemInfo>();
            var files = new List<FileSystemInfo>();
            // folders then files
            foreach (var child in Children.OrderBy(x => x.Name))
            {
                if (child is FolderInfo f)
                {
                    folders.Add(child);
                    continue;
                }

                files.Add(child);
            }

            return folders.Concat(files);
        }
    }
}