namespace wikd.Http;

internal struct HttpStatusCode
{
    internal const string OK = "200 OK";
    internal const string ServerError = "500 Internal Sever Error";
    internal const string ClientError = "400 Bad Request";
    internal const string NotFound = "404 Not Found";
    internal const string Forbidden = "401 Forbidden";
}
