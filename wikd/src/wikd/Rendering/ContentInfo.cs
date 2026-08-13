using wikd.Http;

namespace wikd.Rendering
{
    public class ContentInfo
    {
        public string Content { get; set; } = "";
        public string ContentType { get; set; } = "text/html";
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    }
}
