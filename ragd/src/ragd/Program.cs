using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ragd.Chunk;
using ragd.Clean;
using ragd.Clean.Text;
using ragd.Embed;
using ragd.Handlers;

namespace ragd
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteBanner();

            var config = new Config();
            var builder = Host.CreateApplicationBuilder();

            builder.Configuration.AddJsonFile("ragd.config", optional: true);
            builder.Configuration.Bind(config);

            builder.Logging.ClearProviders();

            builder.Services
                .AddSingleton<ILogger, RagLogger>()
                .AddSingleton(config)
                .AddSingleton<MarkdownChunkCleaner>()
                .AddSingleton<CondenseWhiteSpaceCleaner>()
                .AddSingleton<MarkdownDocumentChunker>()
                .AddSingleton<IEmbedder, Embedder>()
                .AddSingleton<IRepository, Repository>(_ => new Repository(config, new()))
                .AddSingleton<Http.Parser>()
                .AddSingleton<IRequestHandler, HelpRequestHandler>()
                .AddSingleton<IRequestHandler, IndexFileRequestHandler>()
                .AddSingleton<IRequestHandler, QueryRequestHandler>()
                .AddHostedService<Daemon>();

            var host = builder.Build();

            host.Run();
        }

        private static void WriteBanner()
        {
            // figlet font is "DOS-Rebel"  apparantly, cool
            Console.WriteLine();
            var c = Console.ForegroundColor;
            Console.ForegroundColor = (ConsoleColor)Random.Shared.Next(1, 16);
            Console.WriteLine(@"                                  █████");
            Console.WriteLine(@"                                 ░░███ ");
            Console.WriteLine(@" ████████   ██████    ███████  ███████ ");
            Console.WriteLine(@"░░███░░███ ░░░░░███  ███░░███ ███░░███ ");
            Console.WriteLine(@" ░███ ░░░   ███████ ░███ ░███░███ ░███ ");
            Console.WriteLine(@" ░███      ███░░███ ░███ ░███░███ ░███ ");
            Console.WriteLine(@" █████    ░░████████░░███████░░████████");
            Console.WriteLine(@"░░░░░      ░░░░░░░░  ░░░░░███ ░░░░░░░░ ");
            Console.WriteLine(@"                     ███ ░███          ");
            Console.WriteLine(@"                    ░░██████           ");
            Console.WriteLine(@"                     ░░░░░░            ");
            Console.ForegroundColor = c;
            Console.WriteLine();
        }
    }
}