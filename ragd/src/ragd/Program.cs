using http;
using LLama.Native;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ragd.Cli;

namespace ragd
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder();
            // TODO: config - needs to be some kind of merge from cli args
            var config = new Config();
            builder.Configuration.AddJsonFile("ragd.config", optional: true);
            builder.Configuration.Bind(config);

            // shut up llama
            NativeLogConfig.llama_log_set((level, message) =>
            {
                // TODO: should delegate to ILogger
                // LLamaLogLevel is a bit odd, after Error comes Continue...
                // which would map to Critical? should be None.
                if (level == LLamaLogLevel.Warning || level == LLamaLogLevel.Error)
                    Console.Write($"[{level}] {message}");
            });

            var cmd = new Commands(
                    new(),
                    new Client(config.Host, config.Port)
                    )
                    .Parse(args!);

            if (!cmd.IsQuiet()) WriteBanner();

            await cmd.InvokeAsync();
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