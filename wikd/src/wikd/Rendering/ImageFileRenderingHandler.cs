using wikd.Routing;

namespace wikd.Rendering
{
    public class ImageFileRenderingHandler : IRenderingHandler
    {
        public string[] SupportedFileExtensions => [".png", ".jpeg", ".jpg", ".bmp", ".gif", ".webp"];

        public async Task<string> HandleAsync(Route input)
        {
            var file = input as FileRoute;
            var bytes = await File.ReadAllBytesAsync(file!.Path);
            var content = Convert.ToBase64String(bytes);
            var html = $"<img alt='{file.Name}' src='data:image/{file.Extension[1..]};base64, {content}'/>";

            return html;
        }
    }
}