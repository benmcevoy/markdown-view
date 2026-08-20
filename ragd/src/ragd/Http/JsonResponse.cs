using System.Text.Json;

namespace ragd.Http;

public record JsonResponse(HttpStatusCode StatusCode) 
{
    public HttpStatusCode StatusCode { get; init; } = StatusCode;
    public Dictionary<string, string> Headers { get; init; }
        = new(StringComparer.OrdinalIgnoreCase)
        {
            {"Content-Type","application/json"}
        };
    public string Message { get; set; } = "";
    public string Status { get; set; } = "ERROR";
    public object Body { get; set; } = new();
    public override string ToString() => AsHttp(this);

    public static string AsHttp(JsonResponse response)
    {
        var body = AsJson(response);

        response.Headers["Content-Length"] = System.Text.Encoding.UTF8.GetByteCount(body).ToString();

        return @$"HTTP/1.1 {response.StatusCode}
{response.Headers.AsHeaders()}

{body}";
    }

    public static string AsJson(JsonResponse response)
    {
        var json = response.Body is string 
            ? response.Body
            : JsonSerializer.Serialize(response.Body);

        return @$"{{
    ""status"": ""{response.Status}"",
    ""message"": ""{response.Message}"",
    ""body"": {json}
}}";
    }

    public static JsonResponse ServerError(string path = "") => new(HttpStatusCode.ServerError)
    {
        Status = "ERROR",
        Message = $"missing handler for '{path}'"
    };
}