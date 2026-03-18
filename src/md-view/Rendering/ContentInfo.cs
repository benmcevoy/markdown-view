namespace MdView.Rendering
{
    public class ContentInfo
    {
        public string Content { get; set; } = "";
        public string ContentType { get; set; } = "text/html";
        public int StatusCode { get; set; } = 200;
        public static ContentInfo NotFound() => new() { StatusCode = 404, Content = "text/plain" };
    }
}



