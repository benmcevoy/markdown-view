namespace wikd.Http;

public record Request
{
    public string Method { get; init; } = HttpMethod.UNSUPPORTED;

    public string Path { get; init; } = "/";

    public Dictionary<string, string> Query { get; init; }
        = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> Headers { get; init; }
        = new(StringComparer.OrdinalIgnoreCase);

    public string Body { get; init; } = "";

    public static Request Unsupported = new();
}