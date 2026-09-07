using System.IO.Pipelines;

namespace http;

/// <summary>
/// Read-only, non seekable stream wrapper
/// </summary>
internal sealed class BodyStream : Stream
{
    private readonly Stream _inner;
    private readonly PipeReader _reader;
    private readonly long _length;
    private long _position;
    private long _remaining;

    public BodyStream(PipeReader reader, long length)
    {
        _inner = reader.AsStream();
        _reader = reader;
        _length = length;
        _position = 0;
        _remaining = length;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ValidateBufferArgs(buffer, offset, count);

        if (_remaining <= 0) return 0;

        count = (int)Math.Min(count, _remaining);

        var read = _inner.Read(buffer, offset, count);

        if (read == 0)
        {
            throw new IOException($"Connection closed after {_position} of {_length} expected body bytes.");
        }

        _remaining -= read;
        _position += read;

        return read;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
            _reader.Complete();
        }

        base.Dispose(disposing);
    }

    private static void ValidateBufferArgs(byte[] buffer, int offset, int count)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (offset < 0 || count < 0 || offset + count > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(count));
    }

    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override bool CanRead => true;
    public override long Length => _length;
    public override long Position { get => _position; set => throw new NotSupportedException(); }
    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
