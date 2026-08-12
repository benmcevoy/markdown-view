using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ragd.Service;
using ragd.Service.Chunk;
using ragd.Service.Clean;
using ragd.Service.Clean.Text;
using ragd.Service.Embed;
using ragd.Service.Handlers;

namespace ragd
{
    public class LifeCycleManager(IPAddress ipAddress, int port)
    {
        private static Mutex? _mutex;
        private const string AppId = $@"Global\78cd32eb-6fe7-4c7a-af1b-ca63b53fbf15";

        public string StartDaemon(FileInfo database, FileInfo model)
        {
            if (IsRunning()) return LifeCycleStates.RUNNING;

            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var shell = isWindows ? "cmd.exe" : "/bin/sh";
            var exe = Assembly.GetExecutingAssembly().Location;

            if (exe.EndsWith(".dll")) exe = $"dotnet {exe}";

            // setup to start "detached"
            var args = isWindows
                ? $"/c start /b \"{exe}\" listen -db \"{database.FullName}\" -m \"{model.FullName}\" -q > nul 2>&1"
                : $"-c \"{exe} listen -db \"{database.FullName}\" -m \"{model.FullName}\" -q > /dev/null 2>&1 &\"";

            Process.Start(new ProcessStartInfo
            {
                FileName = shell,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            return LifeCycleStates.STARTING;
        }

        public string Listen(FileInfo databasePath, FileInfo modelPath)
        {
            _mutex = new Mutex(true, AppId, out var createdNew);

            if (!createdNew) return LifeCycleStates.RUNNING;

            var builder = Host.CreateApplicationBuilder();

            // TODO: entire config should have been recieved as args
            var config = new Config
            {
                DatabasePath = databasePath.FullName,
                ModelPath = modelPath.FullName,
                Host = ipAddress,
                Port = port,
                VectorExtensionPath = Path.Combine(AppContext.BaseDirectory, "vec0.so")
            };

            // TODO: this DI is gross, extract, raise higher, configure
            // should be using a scope so as to not mess with the cli
            // see also: Cli.NewCommand
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
                .AddSingleton<IRequestHandler, StatusRequestHandler>()
                .AddSingleton<IRequestHandler, StopRequestHandler>()
                .AddHostedService<Daemon>();

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