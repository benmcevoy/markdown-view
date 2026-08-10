using System.Text.Json;

namespace ragd.Http;

public record JsonResponse(HttpStatusCode StatusCode) 
{
    public HttpStatusCode StatusCode { get; init; } = StatusCode;
    public string Message { get; set; } = "";
    public string Status { get; set; } = "ERROR";
    public object Body { get; set; } = new();

    public T? BodyAs<T>() => Body is JsonElement json
         ? json.Deserialize<T>()
         : (T)Body;

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

    public static string AsText(JsonResponse response) => @$"Status: {response.Status}
{response.Message}";

    public static JsonResponse ServerError = new(HttpStatusCode.ServerError)
    {
        Status = "ERROR",
        Message = "missing handler"
    };

    public static JsonResponse DaemonNotRunning = new(HttpStatusCode.ServerError)
    {
        Status = "ERROR",
        Message = "Daemon is not running."
    };
}