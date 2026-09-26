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
using System.Text.Json;
using System.Text;
using ragd.Core;


namespace ragd
{
    public class LifeCycleManager
    {
        private static Mutex? _mutex;
        private const string AppId = $@"Global\78cd32eb-6fe7-4c7a-af1b-ca63b53fbf15";

        public string StartDaemon(Config config)
        {
            if (IsRunning()) return LifeCycleStates.RUNNING;

            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var shell = isWindows ? "cmd.exe" : "/bin/sh";
            var exe = Assembly.GetExecutingAssembly().Location;

            if (exe.EndsWith(".dll")) exe = $"dotnet {exe}";

            var config64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(config)));

            // setup to start "detached"
            var args = isWindows
                ? $"/c start /b \"{exe}\" listen -q --config {config64} > nul 2>&1"
                : $"-c \"{exe} listen -q --config {config64} > /dev/null 2>&1 &\"";

            Process.Start(new ProcessStartInfo
            {
                FileName = shell,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            return LifeCycleStates.STARTING;
        }

        public string Listen(Config config)
        {
            _mutex = new Mutex(true, AppId, out var createdNew);

            if (!createdNew) return LifeCycleStates.RUNNING;

            var builder = Host.CreateApplicationBuilder();

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
                .AddSingleton<IIndexer, Indexer>()
                .AddSingleton<IRepository, Repository>()
                .AddSingleton<IRequestHandler, HelpRequestHandler>()
                .AddSingleton<IRequestHandler, IndexFileRequestHandler>()
                .AddSingleton<IRequestHandler, QueryRequestHandler>()
                .AddSingleton<IRequestHandler, StatusRequestHandler>()
                .AddSingleton<IRequestHandler, StopRequestHandler>()
                .AddSingleton<IRequestHandler, ConfigRequestHandler>()
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