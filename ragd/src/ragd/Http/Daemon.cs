using Microsoft.Extensions.Hosting;
using http;
using ragd.Http.Handlers;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ragd.Core;

namespace ragd.Http
{
    public class HttpBackgroundService : BackgroundService
    {
        private readonly IEnumerable<IRequestHandler> _handlers;
        private readonly Config _config;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _host;
        private IDaemon _daemon;
        private static readonly Response _notFound = new(HttpStatusCode.NotFound);
        static HttpBackgroundService() => _notFound.SetBody("not found");

        public HttpBackgroundService(IDaemon daemon, IEnumerable<IRequestHandler> handlers, Config config, ILogger<Daemon> logger, IHostApplicationLifetime host)
        {
            _handlers = handlers;
            _config = config;
            _logger = logger;
            _host = host;
            _daemon = daemon;
            _daemon.ReceiveAsync = ReceiveAsync;
        }

        private async Task<Response> ReceiveAsync(Request request, CancellationToken cancellationToken)
        {
            var path = string.IsNullOrWhiteSpace(request.Path) ? "/" : request.Path;
            _logger.LogInformation($"> {request.Method} {path}");

            foreach (var r in _handlers)
            {
                if (r.CanHandle(request))
                {
                    var response = r.Handle(request);
                    var result = Map(response);

                    _logger.LogInformation($"< {request.Method} {path} - {response.Status}");

                    if (StopRequestHandler.IsStopRequest(request))
                    {
                        _host.StopApplication();
                    }

                    return result;
                }
            }

            _logger.LogInformation($"< {request.Method} {path} - 404");

            return _notFound;
        }

        private static Response Map(IApiResponse<object> response)
        {
            var result = new Response(HttpStatusCode.OK);

            result.Headers["Content-Type"] = "application/json";

            result.SetBody(JsonSerializer.Serialize(new
            {
                response.Message,
                response.Status,
                response.Data
            }, JsonSerializerOptions.Web));

            return result;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RAG daemon starting");
            _logger.LogInformation(_config.ToString());
            _logger.LogInformation($"Listening on http://{_config.Host}:{_config.Port}/");

            await _daemon.StartAsync(stoppingToken);

            _logger.LogInformation("RAG daemon stopping");
        }
    }
}