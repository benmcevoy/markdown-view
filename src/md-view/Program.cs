using MdView.Rendering;
using System.Diagnostics;
using MdView.Templates;

namespace MdView
{
    class Program
    {
        // TODO: be config and current folder
        // IRenderingHandler should expose string[] of support extension
        private static readonly string[] _allowedFileExtensions = [
            ".md", ".xml", ".json", ".js", ".ts", ".cs", ".jpeg", ".jpg", ".bmp", ".bmp", ".png", ".webp", ".pdf", ".html", ".sh", ".ps1"];

        static async Task Main(string[] args)
        {
            WriteBanner();

            var rootFolder = ParseArgs(args);

            rootFolder = "/home/agent/hello-world/sample";

            Console.WriteLine($"using '{rootFolder}'.");

            var fileSystemInfoService = new FileSystemInfoService(rootFolder, _allowedFileExtensions);
            var fileSystem = fileSystemInfoService.Build();

            var navigation = new Navigation(fileSystem);
            var router = new Router(fileSystem);
            var renderer = new Renderer(new DefaultTemplate(), navigation, [
                new MarkdownFileRendererHandler(), 
                new CodeFileRendererHandler(),
                new ImageFileRendererHandler(),
                new PdfFileRendererHandler(),
                new FolderRenderingHandler()
                ]);

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

                                context.Response.ContentType = "text/html";
                                context.Response.StatusCode = 200;
                                await context.Response.WriteAsync(response);
                            });
                        });
                });

            var host = webBuilder.Build();

            Console.WriteLine("Starting Kestrel host...");
            Console.WriteLine("Listening on: http://localhost:5001");

            StartBrowser("http://localhost:5001");

            await host.RunAsync();
        }

        private static void WriteBanner()
        {
Console.WriteLine(@" _ __ ___   __| |    __   _(_) _____      __");
Console.WriteLine(@"| '_ ` _ \ / _` |____\ \ / / |/ _ \ \ /\ / /");
Console.WriteLine(@"| | | | | | (_| |_____\ V /| |  __/\ V  V / ");
Console.WriteLine(@"|_| |_| |_|\__,_|      \_/ |_|\___| \_/\_/  ");         
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

