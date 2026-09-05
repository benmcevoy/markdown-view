using System.Text;

namespace http.Tests;

public class RequestTests
{
    [Fact]
    public async Task Foo()
    {
        // arrange
        var sut = new Parser();

        // act
        var actual = await sut.ParseRequestAsync(JsonPost(), TestContext.Current.CancellationToken);

        var sr = new StreamReader(actual.Body);
        var text = sr.ReadToEnd();

        // assert


    }

    private static Stream UrlEncodedPost(string body)=>
        AsStream(@$"POST / HTTP/1.1
Host: localhost
Content-Type: application/x-www-form-urlencoded
Content-Length: 27

field1=value1&field2=value2        
");

    private static Stream JsonPost()=>
        AsStream(@$"POST / HTTP/1.1
Host: localhost
Content-Type: application/x-www-form-urlencoded
Content-Length: 16

{{ ""foo"": ""this is some lnger value to see what happens. this is some lnger value to see what happens. 
this is some lnger value to see what happens. this is some lnger value to see what happens. 
this is some lnger value to see what happens. this is some lnger value to see what happens. 
this is some lnger value to see what happens. this is some lnger value to see what happens. 
this is some lnger value to see what happens. this is some lnger value to see what happens. 
this is some lnger value to see what happens. this is some lnger value to see what happens. 
this is some lnger value to see what happens. this is some lnger value to see what happens. 
this is some lnger value to see what happens. this is some lnger value to see what happens. 
this is some lnger value to see what happens. this is some lnger value to see what happens. 
this is some lnger value to see what happens. this is some lnger value to see what happens. 
this is some lnger value to see what happens. this is some lnger value to see what happens. 
this is some lnger value to see what happens. this is some lnger value to see what happens. 
this is some lnger value to see what happens. this is some lnger value to see what happens. 
this is some lnger value to see what happens. this is some lnger value to see what happens. "" }}");

    private static MemoryStream AsStream(string request) =>
        new MemoryStream(Encoding.UTF8.GetBytes(request));
}