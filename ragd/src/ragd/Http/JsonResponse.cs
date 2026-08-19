using System.Text.Json;

namespace ragd.Http;

public record JsonResponse(HttpStatusCode StatusCode) 
{
    public HttpStatusCode StatusCode { get; init; } = StatusCode;
    public Dictionary<string, string> Headers { get; init; }
        = new(StringComparer.OrdinalIgnoreCase);
    public string Message { get; set; } = "";
    public string Status { get; set; } = "ERROR";
    public object Body { get; set; } = new();
    public override string ToString() => AsHttp(this);

    public static string AsHttp(JsonResponse response)
    {
        var body = AsJson(response);

        return @$"HTTP/1.1 {response.StatusCode}
Content-Type: application/json
Content-Length: {body.Length}

{body}";
    }

    public static string AsJson(JsonResponse response)
    {
        return @$"{{
    ""status"": ""{response.Status}"",
    ""message"": ""{response.Message}"",
    ""body"": {JsonSerializer.Serialize(response.Body)}
}}";
    }

    public static JsonResponse ServerError = new(HttpStatusCode.ServerError)
    {
        Status = "ERROR",
        Message = "missing handler"
    };
}