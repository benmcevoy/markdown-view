using wikd.Http;

namespace wikd.Routing
{
    public class SpecialRoute : Route
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.ClientError;
        public Dictionary<string, string> Query { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    }
}