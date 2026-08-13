namespace wikd.Http;

public class Response(HttpStatusCode statusCode)
{
    public HttpStatusCode StatusCode { get; init; } = statusCode;
    public string Message { get; set; } = "";
    public string Status { get; set; } = statusCode.Status;
    public string Body { get; set; } = "";
}
