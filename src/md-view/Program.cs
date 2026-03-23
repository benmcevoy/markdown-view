using MdView.Rendering;
using System.Diagnostics;
using MdView.Templates;

namespace MdView
{
    class Program
    {
        // TODO: be config and current folder
        private static readonly string[] _allowedFileExtensions = [".md"];

        static async Task Main(string[] args)
        {
            Console.WriteLine("md-view started.");

            // TODO: remove this
            args = ["/home/agent/hello-world-docs"];

            var rootFolder = ParseArgs(args);

            Console.WriteLine($"using '{rootFolder}'.");

            var fileSystemInfoService = new FileSystemInfoService(rootFolder, _allowedFileExtensions);
            var fileSystem = fileSystemInfoService.Build();
            var fileRenderer = new MarkdownFileRenderer();
            var router = new Router(fileSystem);
            var renderer = new Renderer(new DefaultTemplate(), [new FileRenderingHandler(fileRenderer), new FolderRenderingHandler(fileRenderer)]);


            var webBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(options =>
                        {
                            options.Listen(System.Net.IPAddress.Loopback, 5001);
                        })
                        .Configure(app =>
                        {
                            app.Run(async context =>
                            {
                                var requestPath = context.Request.Path.ToString();
                                var route = router.Map(requestPath);
                                var response = renderer.Render(route);

                                context.Response.ContentType = response.ContentType;
                                context.Response.StatusCode = response.StatusCode;
                                await context.Response.WriteAsync(response.Content);
                            });
                        });
                });

            var host = webBuilder.Build();

            Console.WriteLine("Starting Kestrel host...");
            Console.WriteLine("Listening on: http://localhost:5001");

            StartBrowser("http://localhost:5001");

            await host.RunAsync();
        }

        private static string ParseArgs(string[] args)
        {
            if (args == null) return Directory.GetCurrentDirectory();
            if (args.Length == 0) return Directory.GetCurrentDirectory();
            if (Directory.Exists(args[0])) return args[0];

            throw new ArgumentException("CLI argument is not a directory?");
        }

        private static void StartBrowser(string targetUrl)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = targetUrl,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it or show an error message)
                Console.WriteLine($"Error opening URL: {ex.Message}");
            }
        }
    }
}

