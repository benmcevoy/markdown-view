using System.IO.Pipelines;

namespace http;

public partial class Parser
{
    public async Task<Request> ParseRequestAsync(Stream requestStream, CancellationToken cancellationToken = default)
    {
        /*
        GET <path>["?"<query>] HTTP/1.1
        headers: and then always a blank line
        
        */

        var body = Stream.Null;
        var pipeReader = PipeReader.Create(requestStream);
        var (method, path, query) = await ParseRequestLine(pipeReader, cancellationToken);
        var headers = await ParseHeaders(pipeReader, cancellationToken);

        if (method == HttpMethod.POST)
        {
            headers.TryGetValue("Content-Length", out var contentLength);
            body = ParseBody(pipeReader, contentLength ?? "");
        }

        return new Request
        {
            Headers = headers,
            Method = method,
            Path = path,
            Query = query,
            Body = body
        };
    }

    public async Task<Response> ParseResponseAsync(Stream responseStream, CancellationToken cancellationToken = default)
    {
        /*
        HTTP/1.1 200 OK
        Content-Type: application/json
        Content-Length: 123

        {
            "status": "{Status}",
            "message": "{Message}",
            "body": "{JsonSerializer.Serialize(Data)}"
        }
        */

        var pipeReader = PipeReader.Create(responseStream);
        var line = await pipeReader.ReadLineAsync(cancellationToken) ?? "";
        var parts = line.Split(Delimiters.Space, StringSplitOptions.RemoveEmptyEntries);
        var statusCode = ParseStatusCode(parts[1]);
        var headers = await ParseHeaders(pipeReader, cancellationToken);

        headers.TryGetValue("Content-Length", out var contentLength);
        var body = ParseBody(pipeReader, contentLength ?? "");

        return new Response(statusCode)
        {
            Headers = headers,
            Body = body
        };
    }

    private static HttpStatusCode ParseStatusCode(string candidate)
    {
        if (candidate.Length != 3) return HttpStatusCode.Malformed;
        if (!int.TryParse(candidate, out var code)) return HttpStatusCode.Malformed;

        return code switch
        {
            >= 200 and < 400 => HttpStatusCode.OK,
            >= 400 and < 500 => HttpStatusCode.ClientError,
            >= 500 and < 600 => HttpStatusCode.ServerError,
            _ => HttpStatusCode.Malformed,
        };
    }

    private static async Task<(HttpMethod, string, Dictionary<string, string>)> ParseRequestLine(PipeReader reader, CancellationToken cancellationToken)
    {
        var line = await reader.ReadLineAsync(cancellationToken) ?? "";
        var parts = line.Split(Delimiters.Space, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3) return (HttpMethod.UNSUPPORTED, "", new());

        var method = ParseMethod(parts[0]);

        if (method == HttpMethod.UNSUPPORTED) return (HttpMethod.UNSUPPORTED, "", new());

        var (path, query) = ParsePathAndQuery(parts[1]);

        return (method, path, query);
    }

    private static HttpMethod ParseMethod(string part)
    {
        return part switch
        {
            "GET" => HttpMethod.GET,
            "POST" => HttpMethod.POST,
            _ => HttpMethod.UNSUPPORTED,
        };
    }

    private static (string, Dictionary<string, string>) ParsePathAndQuery(string part)
    {
        var partLength = part.Length;
        // skip leading slash, start at 1
        var start = (part[0] == Delimiters.Slash) ? 1 : 0;
        var length = part.IndexOfAny([Delimiters.Space, Delimiters.Query, Delimiters.Fragment]);

        length = length == -1 ? partLength : length;

        var path = part[start..length];

        // continue scanning for
        // ?foo=bar&baz=boo
        if (length >= partLength) return (path, new());
        if (part[length] != Delimiters.Query) return (path, new());

        // skip the leading "?"
        var queryStart = length + 1;

        length = part.IndexOfAny([Delimiters.Space, Delimiters.Fragment], startIndex: queryStart);
        length = length == -1 ? (partLength - queryStart) : length - 1;

        var query = part[queryStart..(queryStart + length)];

        return (path, QueryStringToDictionary(query));
    }

    private static async Task<Dictionary<string, string>> ParseHeaders(PipeReader reader, CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        while (true)
        {
            var line = await reader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(line)) break;

            var (key, value) = KeyValue(line, Delimiters.Colon);

            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("invalid header key");

            key = key.Trim();
            value = value.Trim();

            // append value if key already exists
            if (result.ContainsKey(key))
            {
                result[key] += $",{value}";
            }
            else
            {
                result.Add(key, value);
            }
        }

        return result;
    }

    private static Dictionary<string, string> QueryStringToDictionary(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(query)) return result;

        query = query.TrimStart(Delimiters.Query);

        var lines = query.Split(Delimiters.Ampersand, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var (key, value) = KeyValue(line, Delimiters.Equal);

            if (string.IsNullOrWhiteSpace(key)) return result;

            key = Uri.UnescapeDataString(key).Replace(Delimiters.Plus, Delimiters.Space).Trim();
            value = Uri.UnescapeDataString(value).Replace(Delimiters.Plus, Delimiters.Space).Trim();

            // append value if key already exists
            if (result.ContainsKey(key))
            {
                result[key] += $",{value}";
            }
            else
            {
                result.Add(key, value);
            }
        }

        return result;
    }

    private static Stream ParseBody(PipeReader reader, string contentLength)
    {
        if (string.IsNullOrWhiteSpace(contentLength)) return Stream.Null;

        return int.TryParse(contentLength, out var length)
            ? new BodyStream(reader, length)
            : Stream.Null;
    }

    private static (string, string) KeyValue(string line, char separator)
    {
        var index = line.IndexOf(separator);

        // no separator, return the key
        if (index == -1) return (line, "");

        return (line[0..index], line[(index + 1)..]);
    }

    struct Delimiters
    {
        internal const char Space = ' ';
        internal const char Query = '?';
        internal const char Fragment = '#';
        internal const char Colon = ':';
        internal const char Ampersand = '&';
        internal const char Equal = '=';
        internal const char Slash = '/';
        internal const char Plus = '+';
    }
}