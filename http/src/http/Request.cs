using System.Text;

namespace http;

public class Request
{
    public HttpMethod Method { get; init; } = HttpMethod.UNSUPPORTED;

    public string Path { get; init; } = "/";

    public Dictionary<string, string> Query { get; init; }
        = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> Headers { get; init; }
        = new(StringComparer.OrdinalIgnoreCase);

    public Stream Body { get; init; } = Stream.Null;

    internal byte[] AsRequestLineAndHeaders()
    {
        if(Body != Stream.Null) Headers["Content-Length"] = $"{Body?.Length ?? 0}";

        return Encoding.UTF8.GetBytes(@$"{Method} {Path}{Query.AsQueryString()} HTTP/1.1
{Headers.AsHeaders()}

");
    }
    
}