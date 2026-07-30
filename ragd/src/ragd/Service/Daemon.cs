using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using ragd.Http;
using ragd.Service.Handlers;

namespace ragd.Service
{
    public class Daemon : BackgroundService
    {
        private readonly TcpListener _listener;
        private readonly IEnumerable<IRequestHandler> _handlers;
        private readonly Parser _parser;

        public Daemon(IEnumerable<IRequestHandler> handlers, Parser parser, Config config)
        {
            _listener = new(config.Host, config.Port);
            _handlers = handlers;
            _parser = parser;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _listener.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                using var client = await _listener.AcceptTcpClientAsync(stoppingToken);
                using var stream = client.GetStream();

                var request = _parser.ParseRequest(stream);
                var response = Response.ServerError;

                using (new WithColor(ConsoleColor.Yellow)) Console.WriteLine(request);

                foreach (var r in _handlers)
                {
                    if (r.CanHandle(request))
                    {
                        response = r.Handle(request);
                        break;
                    }
                }

                using (new WithColor(ConsoleColor.Yellow)) Console.WriteLine(response);

                var msg = System.Text.Encoding.UTF8.GetBytes(response.ToString());

                await stream.WriteAsync(msg, stoppingToken);
                await stream.FlushAsync(stoppingToken);

                if (StopRequestHandler.IsStopRequest(request))
                {
                    // triggers the SIGTERM and StopAsync
                    Process.GetCurrentProcess().Kill(true);
                }
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _listener.Stop();
            _listener.Dispose();

            return base.StopAsync(cancellationToken);
        }
    }
}