using MdView.Rendering;
using System.Diagnostics;
using MdView.Templates;

namespace MdView
{
    class Program
    {
        static async Task Main(string[] args)
        {
            WriteBanner();

            var rootFolder = ParseArgs(args);
            // TODO:
            // - images do net render when in markdown - should be converted to inline data:base64
            // - use config
            // - admin page
            // - default file is README.md, followed by index.md
            // - FileSystemInfoService has unusual semantics
            // - Error handling, 404 page
            // - security middleware
            // - Admin > refresh file system cache - call Build()
            // - Admin > show config
            // - Admin > stop the server
            // - Admin > show handlers and associated file extensions supported
            // - sometimes larger files have issues? maybe render "too big" instead
            // - use async?
            // - shake the tree, can I get the exe size smaller?

            var ipAddress = System.Net.IPAddress.Loopback;
            var port = 5001;

            //rootFolder = "/home/agent/hello-world/sample";

            Console.WriteLine($"using '{rootFolder}'.");

            IRenderingHandler[] renderers = [
                new MarkdownFileRendererHandler(),
                new CodeFileRendererHandler(),
                new ImageFileRendererHandler(),
                new PdfFileRendererHandler(),
                new FolderRenderingHandler()
                ];

            var allowedFileExtensions = renderers.SelectMany(x=>x.SupportedFileExtensions).ToArray();
            var fileSystemInfoService = new FileSystemInfoService(rootFolder, allowedFileExtensions);
            var navigation = new Navigation(fileSystemInfoService);
            var router = new Router(fileSystemInfoService);
            var renderer = new Renderer(new DefaultTemplate(), navigation, renderers);

            var webBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(options =>
                        {
                            options.Listen(ipAddress, port);
                        })
                        .Configure(app =>
                        {
                            app.Run(async context =>
                            {
                                var requestPath = context.Request.Path.ToString();
                                var route = router.Map(requestPath);
                                var response = renderer.Render(route);

                                context.Response.ContentType = "text/html";
                                context.Response.StatusCode = 200;
                                await context.Response.WriteAsync(response);
                            });
                        });
                });

            var host = webBuilder.Build();

            Console.WriteLine("Starting Kestrel host...");
            Console.WriteLine($"Listening on: http://{ipAddress}:{port}");

            StartBrowser($"http://{ipAddress}:{port}");

            await host.RunAsync();
        }

        private static void WriteBanner()
        {
            Console.WriteLine();
            Console.WriteLine(@" _ __ ___   __| |  __   _(_) _____      __");
            Console.WriteLine(@"| '_ ` _ \ / _` |__\ \ / / |/ _ \ \ /\ / /");
            Console.WriteLine(@"| | | | | | (_| |___\ V /| |  __/\ V  V / ");
            Console.WriteLine(@"|_| |_| |_|\__,_|    \_/ |_|\___| \_/\_/  ");
            Console.WriteLine();
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

