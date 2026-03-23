namespace MdView
{
    public abstract class FileSystemInfo
    {
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public string RequestPath { get; set; } = "";
        public bool IsCurrent { get; set; } = false;

        public override string ToString()
        {
            return $"{Name} ({Path})";
        }
    }

    public class FolderInfo : FileSystemInfo
    {
        public List<FileSystemInfo> Children { get; set; } = [];
    }

    public class FileInfo : FileSystemInfo
    {
        public string Extension { get; set; } = "";
    }
}