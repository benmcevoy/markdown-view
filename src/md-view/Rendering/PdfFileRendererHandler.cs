namespace MdView.Rendering
{
    public class PdfFileRendererHandler : IRenderingHandler
    {
        public bool CanHandle(FileSystemInfo input) =>
            input is FileInfo f && f.Extension == ".pdf";

        public string Handle(FileSystemInfo input)
        {
            var file = input as FileInfo;
            var bytes = File.ReadAllBytes(file!.Path);
            var content = Convert.ToBase64String(bytes);
            var html = $"<iframe src='data:application/pdf;base64, {content}' width='100%' height='90%' style='border:none;'></iframe>";

            return html;
        }
    }
}