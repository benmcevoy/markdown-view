namespace MdView.FileSystem
{
    public abstract class FileSystemInfo
    {
        public FolderInfo? Parent { get; set; } = null;
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public string Uri { get; set; } = "";
        public string FileSystemInfoType() => (this is FolderInfo) ? "folder" : "file";
    }
}