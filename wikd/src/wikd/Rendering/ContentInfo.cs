using http;

namespace wikd.Rendering;

public class ContentInfo : Response
{
    public ContentInfo(HttpStatusCode statusCode) : base(statusCode) 
        => Headers["Content-Type"] = "text/html";
}
