using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ragd.Http;
using ragd.Http.Handlers;
using ragd.Core.Chunk;
using ragd.Core.Clean;
using ragd.Core.Clean.Text;
using ragd.Core.Embed;
using LLama.Native;
using Microsoft.Extensions.Configuration;
using http;


namespace ragd
{
    public class LifeCycleManager
    {
        private static Mutex? _mutex;
        private const string AppId = $@"Global\78cd32eb-6fe7-4c7a-af1b-ca63b53fbf15";

        public string StartDaemon(FileInfo? database, FileInfo? model)
        {
            if (IsRunning()) return LifeCycleStates.RUNNING;

            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var shell = isWindows ? "cmd.exe" : "/bin/sh";
            var exe = Assembly.GetExecutingAssembly().Location;

            if (exe.EndsWith(".dll")) exe = $"dotnet {exe}";

            // handle optional args
            var db = database is null ? "" : $"-db \"{database.FullName}\"";
            var m = model is null ? "" : $"-m \"{model.FullName}\"";

            // setup to start "detached"
            var args = isWindows
                ? $"/c start /b \"{exe}\" listen {db} {m} -q > nul 2>&1"
                : $"-c \"{exe} listen {db} {m} -q > /dev/null 2>&1 &\"";

            Process.Start(new ProcessStartInfo
            {
                FileName = shell,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            return LifeCycleStates.STARTING;
        }

        public string Listen(FileInfo? databasePath, FileInfo? modelPath)
        {
            _mutex = new Mutex(true, AppId, out var createdNew);

            if (!createdNew) return LifeCycleStates.RUNNING;

            var builder = Host.CreateApplicationBuilder();

            // TODO: config - needs to be some kind of merge from default + json +  cli args
            var config = new Config();
            builder.Configuration.AddJsonFile("ragd.config", optional: true);
            builder.Configuration.Bind(config);

            config.DatabasePath = databasePath?.FullName ?? config.DatabasePath;
            config.ModelPath = modelPath?.FullName ?? config.ModelPath;

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

                return "unable to start";
            }

            // TODO: this DI is gross, extract, raise higher, configure
            // should be using a scope so as to not mess with the cli
            // see also: Cli.NewCommand
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
                .AddSingleton<Core.IRepository, Core.Repository>()
                .AddSingleton<IRequestHandler, HelpRequestHandler>()
                .AddSingleton<IRequestHandler, IndexFileRequestHandler>()
                .AddSingleton<IRequestHandler, QueryRequestHandler>()
                .AddSingleton<IRequestHandler, StatusRequestHandler>()
                .AddSingleton<IRequestHandler, StopRequestHandler>()
                .AddSingleton<IDaemon, Daemon>((s) => new Daemon(config.Host, config.Port))
                .AddHostedService<HttpBackgroundService>();

            var host = builder.Build();

            host.Run();

            return LifeCycleStates.STOPPED;
        }

        public bool IsRunning()
        {
            using var mutex = new Mutex(true, AppId, out var createdNew);

            return !createdNew;
        }
    }

    public struct LifeCycleStates
    {
        public const string STARTING = "Starting";
        public const string RUNNING = "Running";
        public const string STOPPED = "Stopped";
    }
}