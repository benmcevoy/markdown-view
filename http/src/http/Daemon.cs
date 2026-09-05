using System.Net;
using System.Net.Sockets;

namespace http;

public interface IDaemon
{
    Task Start(CancellationToken cancellationToken = default);
}

public class Daemon : IDaemon
{
    private readonly TcpListener _listener;
    private readonly Parser _parser;

    public required Func<Request, Response> Receive { get; init; }

    public Daemon(IPAddress host, int port)
    {
        _parser = new();
        _listener = new(host, port);
    }

    public async Task Start(CancellationToken cancellationToken = default)
    {
        _listener.Start();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var client = await _listener.AcceptTcpClientAsync();
                using var stream = client.GetStream();

                var request = await _parser.ParseRequestAsync(stream, cancellationToken);
                var response = new Response(HttpStatusCode.ClientError);

                if (IsValidRequest(request))
                {
                    try
                    {
                        response = Receive(request);
                    }
                    catch (Exception ex)
                    {
                        response = Response.ServerError($"Error handling request for {request.Path} - {ex.Message}");
                    }
                }

                await stream.WriteAsync(response.AsResponseLineAndHeaders(), cancellationToken);
                await response.Body.CopyToAsync(stream, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (SocketException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        Stop();
    }

    public void Stop() => _listener.Stop();

    private static bool IsValidRequest(Request request)
    {
        // Sanitise
        if (request == null) return false;
        if (request.Path == null) return false;

        var requestUrl = request.Path;

        // path traversal is forbidden, i.e. ./../, throw new NotSupportedException
        if (requestUrl.Contains("./") ||
            requestUrl.Contains("/.") ||
            requestUrl.Contains(@"\")) return false;

        return true;
    }
}
