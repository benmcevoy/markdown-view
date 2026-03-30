using FileInfo = MdView.FileSystem.FileInfo;
using FileSystemInfo = MdView.FileSystem.FileSystemInfo;

namespace MdView.Rendering
{
    public class ImageFileRendererHandler : IRenderingHandler
    {
        public string[] SupportedFileExtensions => [".png", ".jpeg", ".jpg", ".bmp", ".gif", ".webp"];

        public string Handle(FileSystemInfo input)
        {
            var file = input as FileInfo;
            var bytes = File.ReadAllBytes(file!.Path);
            var content = Convert.ToBase64String(bytes);
            var html = $"<img alt='{file.Name}' src='data:image/{file.Extension[1..]};base64, {content}'/>";

            return html;
        }
    }
}