using System.Text.Json;
using http;

namespace ragd.Handlers;

public class JsonResponse : Response
{
    public JsonResponse(HttpStatusCode statusCode) : base(statusCode) =>
    Headers = new(StringComparer.OrdinalIgnoreCase) { { "Content-Type", "application/json" } };

    public JsonResponse(HttpStatusCode statusCode, string bodyAsJsonString) : this(statusCode) =>
        SetBody(bodyAsJsonString);

    public JsonResponse(HttpStatusCode statusCode, byte[] body) : this(statusCode) =>
        SetBody(body);

    public string Message { get; set; } = "";
    public string Status { get; set; } = "ERROR";
}

public class JsonResponse<T> : JsonResponse
{
    public JsonResponse(HttpStatusCode statusCode, T body) : base(statusCode) =>
        SetBody(body);

    public void SetBody(T body) =>
        SetBody(JsonSerializer.SerializeToUtf8Bytes(body));
}