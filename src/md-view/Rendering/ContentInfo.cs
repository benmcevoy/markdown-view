namespace MdView.Rendering
{
    public class ContentInfo(string content)
    {
        public string Content { get; set; } = content;
        public string ContentType { get; set; } = "text/html";
        public int StatusCode { get; set; } = 200;
    }
}



