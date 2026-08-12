using System.Text;
using ragd.Http;

namespace ragd.Tests.Http;

public class ParserTests
{

    [Theory]
    [InlineData("test", "test")]
    [InlineData("path/sub", "path/sub")]
    // TODO: normalization issues
    //[InlineData("/test", "/test")]
    //[InlineData("path+with%20space", "path%20with%20space")] ???
    //[InlineData("", "/")]
    public void Parser_ParserRequest_GET_Path(string path, string expected)
    {
        // arrange
        var sut = new Parser();

        // act
        var request = sut.ParseRequest(AsStream($"GET {path} HTTP/1.1"));

        // assert
        Assert.Equal(expected, request.Path);
    }

    [Theory]
    [ClassData(typeof(QueryStringDataFixture))]
    public void Parser_ParserRequest_GET_querystring(string query, Dictionary<string, string> expected)
    {
        // arrange
        var sut = new Parser();

        // act
        var request = sut.ParseRequest(AsStream($"GET {query} HTTP/1.1"));

        // assert
        Assert.Equal(expected, request.Query);
    }

    private static MemoryStream AsStream(string request) =>
        new MemoryStream(Encoding.UTF8.GetBytes(request));

}
