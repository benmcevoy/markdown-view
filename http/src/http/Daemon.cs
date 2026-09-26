using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
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
    public Func<Request, CancellationToken, Task<Response>> ReceiveAsync
    {
        get => _receiveAsync ?? throw new InvalidOperationException("Daemon.ReceiveAsync must be set before use.");
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

        // register _listener for cancellation
        // AcceptTcpClientAsync in netsatandrd2.1 does not have canellation overload
        // so this mechanism address that
        using var listenerCancellationRegisteration = cancellationToken.Register(() => _listener.Stop());

        while (!cancellationToken.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
            {
                // listener was stopped due to cancellation
                break;
            }

            var _ = HandleConnectionAsync(client, cancellationToken);
        }

        await StopAsync(cancellationToken);
    }

    private async Task HandleConnectionAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            var stream = client.GetStream();
            var request = await _parser.ParseRequestAsync(client, stream, cancellationToken);
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
        catch (Exception ex) when (ex is IOException or SocketException or OperationCanceledException)
        {
            // connection dropped or shutting down
            // ignore
            // thanks Claude.
        }
        finally
        {
            client.Dispose();
        }
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
