using Microsoft.Extensions.Hosting;
using http;
using ragd.Handlers;
using Microsoft.Extensions.Logging;

namespace ragd
{
    public class Daemon : BackgroundService
    {
        private readonly IEnumerable<IRequestHandler> _handlers;
        private readonly Config _config;
        private readonly ILogger _logger;
        private http.Daemon _daemon;

        public Daemon(IEnumerable<IRequestHandler> handlers, Config config, ILogger<Daemon> logger)
        {
            _handlers = handlers;
            _config = config;
            _logger = logger;

            _daemon = new(config.Host, config.Port)
            {
                RequestHandler = HandleRequest
            };
        }

        Response HandleRequest(Request request)
        {
            _logger.LogInformation(request.ToString());

            foreach (var r in _handlers)
            {
                if (r.CanHandle(request))
                {
                    return r.Handle(request);
                }
            }

            return new Response(HttpStatusCode.NotFound);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daemon starting");
            _logger.LogInformation(_config.ToString());
            _logger.LogInformation($"Listening on http://{_config.Host}:{_config.Port}/");

            await _daemon.Start(stoppingToken);

            _logger.LogInformation("daemon stopping");
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _daemon.Dispose();

            return base.StopAsync(cancellationToken);
        }
    }
}