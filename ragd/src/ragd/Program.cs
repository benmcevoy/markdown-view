using LLama.Native;
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
        static async Task Main(string[] args)
        {
            WriteBanner();

            var config = new Config();
            var builder = Host.CreateApplicationBuilder();

            builder.Configuration.AddJsonFile("ragd.config", optional: true);
            builder.Configuration.Bind(config);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // shut up llama
            NativeLogConfig.llama_log_set((level, message) =>
            {
                // TODO: should delegate to ILogger
                // LLamaLogLevel is a bit odd, after Error comes Continue...
                // which would map to Critical? should be None.
                if (level == LLamaLogLevel.Warning || level == LLamaLogLevel.Error)
                    Console.Write($"[{level}] {message}");
            });

            var chunkerFactory = (IServiceProvider s) =>
                new ContextSizeAdaptiveDocumentChunker(new MarkdownDocumentChunker(new()), s.GetRequiredService<IEmbedder>());

            builder.Services
                .AddLogging()
                .AddSingleton(config)
                .AddSingleton<CondenseWhiteSpaceCleaner>()
                .AddSingleton<QueryResultCleaner>()
                .AddSingleton<IDocumentChunker>(chunkerFactory)
                .AddSingleton<IEmbedder, Embedder>()
                .AddSingleton<IRepository, Repository>()
                .AddSingleton<Http.Parser>()
                .AddSingleton<IRequestHandler, HelpRequestHandler>()
                .AddSingleton<IRequestHandler, IndexFileRequestHandler>()
                // TODO: perhaps here a chain of responsibility/decorator 
                // to handle the adaptive chunks, 
                // - join chunks
                // - remove duplicate documents found
                // - seems like the spot to think about re-ranker
                /*
Grouping-by-parent-section at retrieval time (then picking the best sub-chunk per section, or merging siblings back together for the generation prompt) is often the fix, and it's basically free if you kept the parent-heading metadata we talked about earlier.
                */
                .AddSingleton<IRequestHandler, QueryRequestHandler>()
                .AddHostedService<Daemon>();

            var host = builder.Build();

            await host.RunAsync();
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