
namespace MdView.Rendering
{
    public interface IRenderingHandler : IHandler<FileSystemInfo, string>
    {
        string[] SupportedFileExtensions { get; }

        bool IHandler<FileSystemInfo, string>.CanHandle(FileSystemInfo input)=>
            input is FileInfo f && SupportedFileExtensions.Contains(f.Extension);
    }
}