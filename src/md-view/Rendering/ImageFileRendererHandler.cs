namespace MdView.Rendering
{
    public class ImageFileRendererHandler : IRenderingHandler
    {
        public string[] SupportedFileExtensions => [".png", ".jpeg", ".jpg", ".bmp", ".gif", ".webp"];

        public bool CanHandle(FileSystemInfo input) =>
            input is FileInfo f && SupportedFileExtensions.Contains(f.Extension);

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