namespace MdView.Rendering
{
    public class PdfFileRendererHandler : IRenderingHandler
    {
        public string[] SupportedFileExtensions => [".pdf"];

        public bool CanHandle(FileSystemInfo input) =>
            input is FileInfo f && SupportedFileExtensions.Contains(f.Extension);
        public string Handle(FileSystemInfo input)
        {
            var file = input as FileInfo;
            var bytes = File.ReadAllBytes(file!.Path);
            var content = Convert.ToBase64String(bytes);
            var html = $"<iframe src='data:application/pdf;base64, {content}' width='100%' style='border:none;height: 100vh'></iframe>";

            return html;
        }
    }
}