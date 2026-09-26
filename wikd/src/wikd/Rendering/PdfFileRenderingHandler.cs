
using wikd.Routing;

namespace wikd.Rendering
{
    public class PdfFileRenderingHandler : IRenderingHandler
    {
        public string[] SupportedFileExtensions => [".pdf"];

        public async Task<string> HandleAsync(Route input)
        {
            var file = input as FileRoute;
            var bytes = await File.ReadAllBytesAsync(file!.Path);
            var content = Convert.ToBase64String(bytes);
            var html = $"<iframe src='data:application/pdf;base64, {content}' width='100%' style='border:none;height: 100vh'></iframe>";

            return html;
        }
    }
}