using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace http;

public interface IDaemon
{
    Func<Request, CancellationToken, Task<Response>> ReceiveAsync { get; set; }
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}

public class Daemon : IDaemon
{
    private readonly TcpListener _listener;
    private readonly Parser _parser;

    private Func<Request, CancellationToken, Task<Response>>? _receiveAsync;

    [DisallowNull]
    public Func<Request, CancellationToken, Task<Response>> ReceiveAsync { 
        get => _receiveAsync ?? throw new InvalidOperationException("Daemon.ReceiveAsync mus be set before use."); 
        set => _receiveAsync = value; 
    }

    /// <summary>
    /// Insantiate Daemon to listen on the given IPAddress and Port
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    /// <remarks>
    /// The ReceiveAsync function MUST be set or will throw InvalidOperationException
    /// </remarks>
    public Daemon(IPAddress host, int port)
    {
        _parser = new();
        _listener = new(host, port);
    }

    public Daemon(IPAddress host, int port, Func<Request, CancellationToken, Task<Response>> receiveAsyncFunc)
        : this(host, port)
    {
        ReceiveAsync = receiveAsyncFunc;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
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
                        response = await ReceiveAsync(request, cancellationToken);
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

        await StopAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _listener.Stop();
        return Task.CompletedTask;
    }

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
