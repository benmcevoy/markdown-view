using MdView.Routing;

namespace MdView.Rendering
{
    public class ImageFileRendererHandler : IRenderingHandler
    {
        public string[] SupportedFileExtensions => [".png", ".jpeg", ".jpg", ".bmp", ".gif", ".webp"];

        public string Handle(Route input)
        {
            var file = input as FileRoute;
            var bytes = File.ReadAllBytes(file!.Path);
            var content = Convert.ToBase64String(bytes);
            var html = $"<img alt='{file.Name}' src='data:image/{file.Extension[1..]};base64, {content}'/>";

            return html;
        }
    }
}