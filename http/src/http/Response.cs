using System.Text;

namespace http;

public class Response(HttpStatusCode statusCode)
{
    public HttpStatusCode StatusCode { get; init; } = statusCode;
    public Dictionary<string, string> Headers { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Stream Body { get; set; } = Stream.Null;

    /// <summary>
    /// Set the Body stream
    /// </summary>
    /// <param name="body"></param>
    public void SetBody(string body) =>
        Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

    public void SetBody(byte[] body) =>
            Body = new MemoryStream(body);

    internal byte[] AsResponseLineAndHeaders()
    {
        if(Body != Stream.Null) Headers["Content-Length"] = $"{Body?.Length ?? 0}";

        return Encoding.UTF8.GetBytes(@$"HTTP/1.1 {StatusCode}
{Headers.AsHeaders()}

");
    }

    internal static Response ServerError(string errorMessage)
    {
        var response = new Response(HttpStatusCode.ServerError);
        response.SetBody(errorMessage);
        return response;
    }
}