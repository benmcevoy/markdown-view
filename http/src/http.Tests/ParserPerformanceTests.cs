using System.Diagnostics;
using System.Text;

namespace http.Tests;

public class ParserPerformanceTests(ITestOutputHelper output)
{

    //[Fact]
    public async Task MeasureAverage_GET_end_to_end()
    {
        var sw = Stopwatch.StartNew();

        var ticks = 0L;
        var iterations = 100000L;
        var request = new Request { Method = HttpMethod.GET };
        var cancellationSource = new CancellationTokenSource();
        var cancelToken = cancellationSource.Token;
        var client = new Client(System.Net.IPAddress.Loopback, 53280);

        new Thread(() =>
         {
             var server = new Daemon(System.Net.IPAddress.Loopback, 53280)
             {
                 Receive = (r) => new Response(HttpStatusCode.OK)
             };
             Task.Run(() => server.StartAsync(cancelToken)).GetAwaiter().GetResult();

         }).Start();

        Thread.Sleep(200);

        for (var i = 0; i < iterations; i++)
        {
            // arrange
            sw.Reset();
            sw.Start();

            // act
            var response = await client.SendAsync(request, cancelToken);

            sw.Stop();
            ticks += sw.ElapsedTicks;

            // assert
            Assert.Equal(response.StatusCode, HttpStatusCode.OK);
        }

        var avg = ticks / (double)iterations;
        output.WriteLine($"{Stopwatch.Frequency} frequency");
        output.WriteLine($"{ticks} ticks");
        output.WriteLine($"{avg / Stopwatch.Frequency * 1000D} ms");
        var rps = (double)iterations / ((double)ticks / (double)Stopwatch.Frequency);
        output.WriteLine($"{rps} requests/sec");

        cancellationSource.Cancel();

        //Assert.Fail();

        // performance number here ~400k in debug (~630k in release), but should be
        // - Release
        // - End to end
        // so probably maybe ~500k/second ish?
        // not too bad
    }

    //[Fact]
    public async Task MeasureAverage_parser_request_time()
    {
        var sw = Stopwatch.StartNew();

        var ticks = 0L;
        var iterations = 1000000L;
        using var request = RequestStream();

        for (var i = 0; i < iterations; i++)
        {
            // arrange
            sw.Reset();
            request.Position = 0;
            sw.Start();
            var sut = new Parser();

            // act
            var actual = await sut.ParseRequestAsync(request, TestContext.Current.CancellationToken);

            sw.Stop();
            ticks += sw.ElapsedTicks;

            // assert
            // Assert.Equal("test", actual.Path);
            // Assert.Equal("hello", actual.Headers["X-Foo-Bar"]);
            // Assert.Equal("b", actual.Query["a"]);
        }

        var avg = ticks / (double)iterations;
        output.WriteLine($"{Stopwatch.Frequency} frequency");
        output.WriteLine($"{ticks} ticks");
        output.WriteLine($"{avg / Stopwatch.Frequency * 1000D} ms");
        var rps = (double)iterations / ((double)ticks / (double)Stopwatch.Frequency);
        output.WriteLine($"{rps} requests/sec");

        //Assert.Fail();

        // performance number here ~400k in debug (~630k in release), but should be
        // - Release
        // - End to end
        // so probably maybe ~500k/second ish?
        // not too bad
    }

    //[Fact]
    public async Task MeasureAverage_parser_response_time()
    {
        var sw = Stopwatch.StartNew();

        var ticks = 0L;
        var iterations = 1000000L;
        using var request = ResponseStream();

        for (var i = 0; i < iterations; i++)
        {
            // arrange
            sw.Reset();
            request.Position = 0;
            sw.Start();
            var sut = new Parser();

            // act
            var actual = await sut.ParseResponseAsync(request, TestContext.Current.CancellationToken);

            sw.Stop();
            ticks += sw.ElapsedTicks;

            // assert
            // Assert.Equal("test", actual.Path);
            // Assert.Equal("hello", actual.Headers["X-Foo-Bar"]);
            // Assert.Equal("b", actual.Query["a"]);
        }

        var avg = ticks / (double)iterations;
        output.WriteLine($"{Stopwatch.Frequency} frequency");
        output.WriteLine($"{ticks} ticks");
        output.WriteLine($"{avg / Stopwatch.Frequency * 1000D} ms");
        var rps = (double)iterations / ((double)ticks / (double)Stopwatch.Frequency);
        output.WriteLine($"{rps} requests/sec");

        //Assert.Fail();

        // performance number here ~400k in debug (~630k in release), but should be
        // - Release
        // - End to end
        // so probably maybe ~500k/second ish?
        // not too bad
    }

    private static MemoryStream RequestStream()
        => new MemoryStream(Encoding.UTF8.GetBytes(@"GET test?a=b&c= HTTP/1.1
User-Agent: test-runner
X-Foo-Bar: hello

"));

    private static MemoryStream ResponseStream()
        => new MemoryStream(Encoding.UTF8.GetBytes(@"HTTP/1.1 200 OK
Content-Type: application/json
Content-Length: 123

{
    ""status"": ""{Status}"",
    ""message"": ""{Message}"",
    ""body"": ""{JsonSerializer.Serialize(Data)}""
}
"));
}