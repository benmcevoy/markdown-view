using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace http;

public static class PipeReaderExtensions
{
    /// <summary>
    /// Read line from underlying stream
    /// </summary>
    /// <param name="pipeReader"></param>
    /// <remarks>Behaves like StreamReader.ReadLine<remarks>
    /// <returns>the line or null if not found</returns>
    public static async Task<string?> ReadLineAsync(this PipeReader pipeReader, CancellationToken cancellationToken = default)
    {
        const byte NewLine = 10;
        const char CarriageReturn = '\r';

        while (true)
        {
            var sequence = await pipeReader.ReadAsync(cancellationToken);
            var sequenceReader = new SequenceReader<byte>(sequence.Buffer);

            var isFound = sequenceReader.TryReadToAny(out ReadOnlySequence<byte> candidate,
                [NewLine],
                advancePastDelimiter: true);

            if (isFound)
            {
                // materialize the string and advance the reader
                var line = Encoding.UTF8.GetString(candidate).TrimEnd(CarriageReturn);
                // MUST advance AFTER materializing bytes
                pipeReader.AdvanceTo(sequenceReader.Position);
                return line;
            }

            if (sequence.IsCompleted)
            {
                // check for trailing unterminated content and return that
                // same as StreamReader
                // expectation is "this is a line" and "this is also a line\n"
                var trailing = sequence.Buffer.Length > 0
                    ? Encoding.UTF8.GetString(sequence.Buffer).TrimEnd(CarriageReturn)
                    : null;

                // advance to the end
                pipeReader.AdvanceTo(sequence.Buffer.End);

                return trailing;
            }

            // keep looking
            pipeReader.AdvanceTo(sequenceReader.Position, examined: sequence.Buffer.End);
        }
    }
}