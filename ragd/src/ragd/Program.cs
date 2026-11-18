using System.CommandLine;
using http;
using LLama.Native;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Hosting;
using ragd.Cli;
using ragd.Cli.Options;

namespace ragd
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder();

            var config = new Config();
            // dotnet does not like .dotfiles
            builder.Configuration.AddJsonFile(
                new PhysicalFileProvider(Directory.GetCurrentDirectory(), ExclusionFilters.None),
                ".ragd", optional: true, reloadOnChange: false);
            builder.Configuration.Bind(config);

            // shut up llama
            NativeLogConfig.llama_log_set((level, message) =>
            {
                // TODO: should delegate to ILogger
                // LLamaLogLevel is a bit odd, after Error comes Continue...
                // which would map to Critical? should be None.
                if (level == LLamaLogLevel.Warning || level == LLamaLogLevel.Error)
                    Console.WriteLine($"[{level}] {message}");
            });

            var cmd = new Commands(
                    config,
                    new(),
                    new Client(config.Host, config.Port)
                    )
                    .Parse(args!);

            // I really dislike mutating the config here :(
            MergeCliOptions(ref config, cmd);

            var (isValid, errors) = config.Validate();
            if (!isValid)
            {
                var c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
                Console.ForegroundColor = c;

                return;
            }

            if (!config.IsQuiet) WriteBanner();

            await cmd.InvokeAsync();
        }

        private static void MergeCliOptions(ref Config config, ParseResult parseResult)
        {
            config.IsQuiet = parseResult.GetValue<bool>(QuietOption.OptionName) ? true : config.IsQuiet;
            config.IsJson = parseResult.GetValue<bool>(JsonOption.OptionName) ? true : config.IsJson;
            config.Name = parseResult.GetValue<string>(NameOption.OptionName) ?? config.Name;
            config.TopNResults = parseResult.GetValue<int?>(LimitOption.OptionName) ?? config.TopNResults;
            config.DatabasePath = parseResult.GetValue<string>(DatabaseOption.OptionName) ?? config.DatabasePath;
            config.ModelPath = parseResult.GetValue<string>(ModelOption.OptionName) ?? config.ModelPath;

            // other config properties are NOT cli options, e.g. host, port

            // express paths as absolute
            config.DatabasePath = new FileInfo(config.DatabasePath).FullName;
            config.ModelPath = new FileInfo(config.ModelPath).FullName;
            config.VectorExtensionPath = new FileInfo(config.VectorExtensionPath).FullName;
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