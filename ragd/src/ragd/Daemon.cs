using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using ragd.Http;
using ragd.Handlers;

namespace ragd
{
    public class Daemon(IEnumerable<IRequestHandler> handlers, Parser parser, Config config) : BackgroundService
    {
        private readonly TcpListener _listener = new(config.Host, config.Port);
        private readonly IEnumerable<IRequestHandler> _handlers = handlers;
        private readonly Parser _parser = parser;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _listener.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                using var client = await _listener.AcceptTcpClientAsync(stoppingToken);
                using var stream = client.GetStream();

                var request = _parser.ParseRequest(stream);
                var response = JsonResponse.ServerError;

                Log(request.ToString(), ConsoleColor.Green);

                foreach (var r in _handlers)
                {
                    if (r.CanHandle(request))
                    {
                        response = r.Handle(request);
                        break;
                    }
                }

                var responseString = response.ToString();

                Log(responseString);

                var msg = System.Text.Encoding.UTF8.GetBytes(responseString);

                await stream.WriteAsync(msg, stoppingToken);
                await stream.FlushAsync(stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _listener.Stop();
            _listener.Dispose();

            return base.StopAsync(cancellationToken);
        }

        private void Log(string message, ConsoleColor color = ConsoleColor.Yellow)
        {
            var original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = original;
        }
    }
}