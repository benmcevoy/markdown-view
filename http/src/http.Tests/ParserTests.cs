using System.Text;
using http.Tests.Fixtures;

namespace http.Tests;

public class ParserTests
{

    [Theory]
    [ClassData(typeof(RequestLineDataFixture))]
    public async Task Parser_ParserRequest_GET_requests(string requestLine, string expectedPath, string expectedQuery)
    {
        // arrange
        var sut = new Parser();

        // act
        var request = await sut.ParseRequestAsync(AsStream(requestLine), TestContext.Current.CancellationToken);

        // assert
        Assert.Equal(expectedPath, request.Path);
        Assert.Equal(expectedQuery, request.Query.AsQueryString());
    }

    [Theory]
    [ClassData(typeof(QueryStringDataFixture))]
    public async Task Parser_ParserRequest_GET_querystring(string query, Dictionary<string, string> expected)
    {
        // arrange
        var sut = new Parser();

        // act
        var request = await sut.ParseRequestAsync(AsStream($"GET {query} HTTP/1.1\n"), TestContext.Current.CancellationToken);

        // assert
        Assert.Equal(expected, request.Query);
    }

    [Theory]
    [ClassData(typeof(HeadersDataFixture))]
    public async Task Parser_ParserRequest_GET_headers(string headers, Dictionary<string, string> expected)
    {
        // arrange
        var sut = new Parser();

        // act
        var request = await sut.ParseRequestAsync(AsStream($"GET / HTTP/1.1\n{headers}\n"), TestContext.Current.CancellationToken);

        // assert
        Assert.Equal(expected, request.Headers, AreDictionariesEquivalent);
    }

    [Fact]
    public async Task Parse_ParseRequest_Can_read_body()
    {
        // arrange
        var sut = new Parser();

        // act
        var result = await sut.ParseRequestAsync(RequestWithBody, TestContext.Current.CancellationToken);

        // assert
        var sr = new StreamReader(result.Body);
        var body = sr.ReadToEnd();

        Assert.Equal(Body, body);
    }

    private static bool AreDictionariesEquivalent(Dictionary<string, string> first, Dictionary<string, string> second)
    {
        if (!first.Keys.SequenceEqual(second.Keys, StringComparer.OrdinalIgnoreCase)) return false;
        return first.Values.SequenceEqual(second.Values, StringComparer.Ordinal);
    }

    private static MemoryStream AsStream(string request) =>
        new MemoryStream(Encoding.UTF8.GetBytes(request));

    private static Stream RequestWithBody =>
    AsStream($@"POST / HTTP/1.1
Content-Length: {Encoding.UTF8.GetByteCount(Body)}

{Body}
");

    private const string Body = @"{
""foo"": ""bar""
    }";
}
