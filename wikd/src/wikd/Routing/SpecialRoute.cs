using wikd.Http;

namespace wikd.Routing
{
    public class SpecialRoute : Route
    {
        public string StatusCode { get; set; } = HttpStatusCode.Forbidden;
        public Dictionary<string, string> Query { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    }
}