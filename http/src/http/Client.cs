using System.Net;
using System.Net.Sockets;

namespace http;

public interface IClient
{
    Task<Response> SendAsync(Request request, CancellationToken cancellationToken = default);
}

public class Client(IPAddress host, int port) : IClient
{
    private readonly Parser _parser = new();

    public async Task<Response> SendAsync(Request request, CancellationToken cancellationToken = default)
    {
        var client = new TcpClient(host.ToString(), port);
        try
        {
            var stream = client.GetStream();

            await stream.WriteAsync(request.AsRequestLineAndHeaders(), cancellationToken);
            await request.Body.CopyToAsync(stream, cancellationToken);
            await stream.FlushAsync(cancellationToken);

            return await _parser.ParseResponseAsync(client, stream, cancellationToken);
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }
}
