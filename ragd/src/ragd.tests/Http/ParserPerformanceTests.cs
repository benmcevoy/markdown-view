

using System.Diagnostics;
using System.Text;
using ragd.Http;

namespace ragd.Test.Http;

public class ParserPerformanceTests(ITestOutputHelper output)
{
    [Fact]
    public void MeasureAverage_GET()
    {
        var sw = Stopwatch.StartNew();
        var ticks = 0L;
        var iterations = 1000000;
        using var request = RequestStream();

        for (var i = 0; i < iterations; i++)
        {
            // arrange
            sw.Reset();
            request.Position = 0;
            sw.Start();
            var sut = new Parser();

            // act
            var actual = sut.ParseRequest(request);

            sw.Stop();
            ticks += sw.ElapsedTicks;

            // assert
            // Assert.Equal("test", actual.Path);
            // Assert.Equal("hello", actual.Headers["X-Foo-Bar"]);
            // Assert.Equal("b", actual.Query["a"]);
        }

        var avg = ticks / (double)iterations;

        output.WriteLine($"{avg} ticks");
        output.WriteLine($"{avg / TimeSpan.TicksPerMillisecond} ms");
    }

    private static MemoryStream RequestStream()
        => new MemoryStream(Encoding.UTF8.GetBytes(@"GET test?a=b&c= HTTP/1.1
User-Agent: test-runner
X-Foo-Bar: hello

"));
}