using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using ragd.Http;
using ragd.Handlers;
using Microsoft.Extensions.Logging;

namespace ragd
{
    public class Daemon(IEnumerable<IRequestHandler> handlers, Parser parser, Config config, ILogger<Daemon> logger) : BackgroundService
    {
        private readonly TcpListener _listener = new(config.Host, config.Port);
        private readonly IEnumerable<IRequestHandler> _handlers = handlers;
        private readonly Parser _parser = parser;
        private readonly ILogger _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("daemon starting");
            _logger.LogInformation(config.ToString());
            _logger.LogInformation($"Listening on http://{config.Host}:{config.Port}/");

            _listener.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var client = await _listener.AcceptTcpClientAsync(stoppingToken);
                    using var stream = client.GetStream();
                    var request = _parser.ParseRequest(stream);
                    var response = JsonResponse.ServerError(request.Path);

                    _logger.LogInformation(request.ToString());

                    foreach (var r in _handlers)
                    {
                        if (r.CanHandle(request))
                        {
                            response = r.Handle(request);
                            break;
                        }
                    }

                    var responseString = response.ToString();

                    _logger.LogInformation(responseString);

                    var msg = System.Text.Encoding.UTF8.GetBytes(responseString);

                    await stream.WriteAsync(msg, stoppingToken);
                    await stream.FlushAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (SocketException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            _logger.LogInformation("daemon stopping");
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _listener.Stop();
            _listener.Dispose();

            return base.StopAsync(cancellationToken);
        }
    }
}