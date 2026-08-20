using System.Text.Json;

namespace ragd.Http;

public class Parser
{
    public Request ParseRequest(Stream requestStream)
    {
        /*
        GET <path>["?"<query>] HTTP/1.1
        headers: and then always a blank line
        
        */

        var body = "";
        var sr = new StreamReader(requestStream);
        var line = sr.ReadLine() ?? "";
        var (method, path, query) = ParseLine(line);
        var headers = ParseHeaders(sr);

        if (method == HttpMethod.POST)
        {
            headers.TryGetValue("Content-Length", out var contentLength);
            body = ParseBody(sr, contentLength ?? "");
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

    public JsonResponse ParseResponse(Stream responseStream)
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

        var sr = new StreamReader(responseStream);
        var line = sr.ReadLine() ?? "";
        var parts = line.Split(Delimiters.Space);
        var statusCode = ParseStatusCode(parts[1..2]);
        var headers = ParseHeaders(sr);

        // TODO: this assumes some json payload
        // but could be formencoded or whatever
        // should check the content-type?
        var response = JsonSerializer.Deserialize<JsonResponse>(sr.BaseStream, JsonSerializerOptions.Web) 
            ?? JsonResponse.ServerError();

        return new JsonResponse(statusCode)
        {
            Headers = headers,
            Body = response.Body,
            Message = response.Message,
            Status = response.Status
        };
    }

    private static HttpStatusCode ParseStatusCode(string[] parts)
    {
        if (parts.Length != 3) return HttpStatusCode.Malformed;
        if (int.TryParse(parts[0], out var code)) return HttpStatusCode.Malformed;

        return code switch
        {
            >= 200 and < 400 => HttpStatusCode.OK,
            >= 400 and < 500 => HttpStatusCode.ClientError,
            >= 500 and < 600 => HttpStatusCode.ServerError,
            _ => HttpStatusCode.Malformed,
        };
    }

    private static (HttpMethod, string, Dictionary<string, string>) ParseLine(string line)
    {
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
        int length;
        var partLength = part.Length;
        var start = (part[0] == Delimiters.Slash) ? 1 : 0;

        for (length = 0; length < partLength; length++)
        {
            var character = part[length];

            if (character == Delimiters.Space ||
                character == Delimiters.Query ||
                character == Delimiters.Fragment)
            {
                break;
            }
        }

        // skip leading slash, start at 1
        var path = part[start..length];

        // continue scanning for
        // ?foo=bar&baz=boo
        if (length >= partLength) return (path, new());
        if (part[length] != Delimiters.Query) return (path, new());

        var queryStart = length + 1;

        for (length = 0; length < partLength - queryStart; length++)
        {
            var character = part[queryStart + length];

            if (character == Delimiters.Space ||
                character == Delimiters.Fragment)
            {
                break;
            }
        }

        var query = part[queryStart..(queryStart + length)];

        return (path, QueryStringToDictionary(query));
    }

    private static Dictionary<string, string> ParseHeaders(StreamReader sr)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        while (!sr.EndOfStream)
        {
            var line = sr.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(line)) break;

            var (key, value) = KeyValue(line, Delimiters.Colon, true);

            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("invalid header key");

            headers.Add(key, value);
        }

        return headers;
    }

    private static string ParseBody(StreamReader sr, string contentLength)
    {
        if (string.IsNullOrWhiteSpace(contentLength)) return "";
        if (sr == null) return "";

        if (int.TryParse(contentLength, out var length))
        {
            var buffer = new char[length];
            sr.Read(buffer, 0, length);

            return new string(buffer);
        }

        return "";
    }

    private static Dictionary<string, string> QueryStringToDictionary(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(query)) return result;

        query = query.TrimStart(Delimiters.Query);

        var lines = query.Split(Delimiters.Ampersand, StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) break;

            var (key, value) = KeyValue(line, Delimiters.Equal, false);

            if (string.IsNullOrWhiteSpace(key)) return result;

            result.Add(Uri.UnescapeDataString(key), Uri.UnescapeDataString(value));
        }

        return result;
    }

    private static (string, string) KeyValue(string line, char separator, bool trimValue)
    {
        // line in form key{separator}[ ]value
        var lineLength = line.Length;
        int length;

        for (length = 0; length < lineLength; length++)
        {
            var character = line[length];

            if (character == separator)
            {
                break;
            }
        }

        if (length >= lineLength) return ("", "");

        var key = line[0..length];
        var value = line[(length + (trimValue ? 2 : 1))..lineLength];

        return (key, value);
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
    }
}