namespace MdView
{
    public abstract class FileSystemInfo
    {
        public FolderInfo? Parent { get; set; } = null;
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public string RequestPath { get; set; } = "";
        public string Uri { get; set; } = "";

        public override string ToString() => $"{Name} ({Path})";
    }

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

    public class FileInfo : FileSystemInfo
    {
        public string Extension { get; set; } = "";
    }
}