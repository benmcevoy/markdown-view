using System.Text;

namespace ragd.Http;

public record Request
{
    public HttpMethod Method { get; init; } = HttpMethod.UNSUPPORTED;

    public string Path { get; init; } = "/";

    public Dictionary<string, string> Query { get; init; }
        = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> Headers { get; init; }
        = new(StringComparer.OrdinalIgnoreCase);

    public string Body { get; init; } = "";

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{Method} {Path}{Query.AsQuery()} HTTP/1.1");

        if (Headers.Count > 0) sb.Append(Headers.AsHeaders());

        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(Body))
        {
            sb.AppendLine(Body);
        }

        return sb.ToString();
    }
}