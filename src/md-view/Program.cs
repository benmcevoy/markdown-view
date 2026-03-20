using MdView.Rendering;
using MdView.Routing;
using MdView.Navigation;
using System.Diagnostics;
using MdView.Templates;

namespace MdView
{
    class Program
    {
        // TODO: be config and current folder
        private static string[] _allowedFileExtenions = [".md"];

        static async Task Main(string[] args)
        {
            // TODO: should try to use arg
            // then current folder by default
            var rootFolder = "/home/agent/hello-world/sample";//Directory.GetCurrentDirectory();


            var router = new Router(rootFolder, _allowedFileExtenions);
            var renderer = new Renderer(new(rootFolder, _allowedFileExtenions), new MarkdownFileRenderer(), new DefaultTemplate());
            var builder = Host.CreateDefaultBuilder(args)
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

            var host = builder.Build();

            Console.WriteLine("Starting Kestrel host...");
            Console.WriteLine("Listening on: http://localhost:5001");

            StartBrowser("http://localhost:5001");

            await host.RunAsync();
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

