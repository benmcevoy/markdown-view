using System.IO.Pipelines;
using System.Text;

namespace http.Test;

public class PipeReaderReadLineTests
{

    // pipereader read produces a buffer
    //         var result = await sut.ReadAsync(TestContext.Current.CancellationToken);

    // sequence reader operates on a buffer

    // var ss = new SequenceReader<byte>(result.Buffer);

    // search the buffer
    // ss.TryReadToAny(,);

    // see where we got up to "index of"
    // var sp = ss.Position;

    // if found move the reader along
    // sut.AdvanceTo(sp);

    // if NOT found move the reader along
    // sut.AdvanceTo(sp, examined: result.Buffer.End);

    // // read now returns the NEXT buffer or a BIGGER buffer (as examined reached the end)
    // // returns data from the consumed point (ss.Position) onwards
    // // repeated reads WITHOUT advance WILL THROW!
    // sut.ReadAsync...;

    // once "found" stop advancing, and the pipereader will stay at that poistion, ready for the Body to streamed out


    [Fact]
    public async Task PipeReader_ReadLine_spike()
    {
        // arrange
        var stream = JsonPost();
        var sut = PipeReader.Create(stream);

        // act
        var actual = await sut.ReadLineAsync(cancellationToken: TestContext.Current.CancellationToken);

        // look for the next line
        var line2 = await sut.ReadLineAsync(cancellationToken: TestContext.Current.CancellationToken);

        // assert
        Assert.Equal("POST / HTTP/1.1", actual);
        Assert.Equal("Host: localhost", line2);
    }

    [Fact]
    public async Task PipeReader_ReadLine_behaves_like_stream_reader()
    {
        // arrange
        var sut = PipeReader.Create(JsonPost());
        var streamReader = new StreamReader(JsonPost());

        for (var i = 0; i < 3; i++)
        {
            // act
            var pipeReaderLine = await sut.ReadLineAsync(cancellationToken: TestContext.Current.CancellationToken);
            var srLine = streamReader.ReadLine();

            Assert.Equal(srLine, pipeReaderLine);
        }
    }

    [Fact]
    public async Task PipeReader_ReadLine_empty_string()
    {
        // arrange
        var sut = PipeReader.Create(AsStream(""));

        // act
        var actual = await sut.ReadLineAsync(cancellationToken: TestContext.Current.CancellationToken);

        // assert
        Assert.Null(actual);

    }

    [Fact]
    public async Task PipeReader_ReadLine_EmptyStream_reads_null()
    {
        // arrange
        var sut = PipeReader.Create(new MemoryStream());

        // act
        var actual = await sut.ReadLineAsync(cancellationToken: TestContext.Current.CancellationToken);

        // assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task PipeReader_ReadLine_JustABlankLine_is_a_blank_line()
    {
        // arrange
        var sut = PipeReader.Create(AsStream("\n"));

        // act
        var actual = await sut.ReadLineAsync(cancellationToken: TestContext.Current.CancellationToken);

        // assert
        Assert.Equal("", actual);
    }

    [Fact]
    public async Task PipeReader_ReadLine_unterminated_line_is_a_line()
    {
        // arrange
        var sut = PipeReader.Create(AsStream("this is a line"));

        // act
        var actual = await sut.ReadLineAsync(cancellationToken: TestContext.Current.CancellationToken);

        // assert
        Assert.Equal("this is a line", actual);
    }

    [Fact]
    public async Task PipeReader_ReadLine_line_is_longer_than_buffer_4096()
    {
        // arrange
        var longLine = "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. " +
            "this is a line, a very long line. a very very long line. this is a line, a very long line. a very very long line. ";

        longLine += longLine;

        Assert.True(longLine.Length > 4096);

        var sut = PipeReader.Create(AsStream(longLine));

        // act
        var actual = await sut.ReadLineAsync();

        // assert
        Assert.Equal(longLine, actual);
    }


    private static Stream JsonPost() =>
      AsStream(@$"POST / HTTP/1.1
Host: localhost
Content-Type: application/x-www-form-urlencoded
Content-Length: 16

{{ ""foo"": ""need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
need big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuveneed big abcdefghijklmnopqrstuve need big abcdefghijklmnopqrstuve
"" }}");

    private static MemoryStream AsStream(string request) =>
        new MemoryStream(Encoding.UTF8.GetBytes(request));
}