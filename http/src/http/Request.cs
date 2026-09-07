using System.Text;

namespace http;

public class Request
{
    public HttpMethod Method { get; init; } = HttpMethod.GET;

    public string Path { get; init; } = "/";

    public Dictionary<string, string> Query { get; init; }
        = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> Headers { get; init; }
        = new(StringComparer.OrdinalIgnoreCase);

    public Stream Body { get; init; } = Stream.Null;

    internal byte[] AsRequestLineAndHeaders()
    {
        if (Body != Stream.Null) Headers["Content-Length"] = $"{Body?.Length ?? 0}";

        var query = Query.AsQueryString();
        
        if (query.Length >= 1) query = "?" + query;

        return Encoding.UTF8.GetBytes(@$"{Method} {Path}{query} HTTP/1.1
{Headers.AsHeaders()}

");
    }

}