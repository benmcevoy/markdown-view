
namespace MdView.Rendering
{
    public interface IRenderingHandler : IHandler<FileSystemInfo, string>
    {
        string[] SupportedFileExtensions { get; }
    }
}