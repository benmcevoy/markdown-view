using MdView.Rendering;
using MdView.Routing;

namespace MdView
{
    class Program
    {
        private const string StaticAssetsPath = "wwwroot";
        private static readonly Router _router = new("/home/agent/hello-world/sample", StaticAssetsPath);
        private static readonly Renderer _renderer = new(new MarkdownFileRenderer(), new MainTemplate(StaticAssetsPath));

        static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel();
                    webBuilder.Configure(app =>
                    {
                        app.Run(async context =>
                        {
                            var content = await Content(context);
                            await context.Response.WriteAsync(content);
                        });
                    });
                });

            var host = builder.Build();

            Console.WriteLine("Starting Kestrel host...");
            Console.WriteLine("Listening on: https://localhost:5001");

            await host.RunAsync();
        }

        private static async Task<string> Content(HttpContext context)
        {
            try
            {
                var requestPath = context.Request.Path.ToString();
                var route = _router.Map(requestPath);
                var response = _renderer.Render(route);

                context.Response.ContentType = response.ContentType;
                context.Response.StatusCode = response.StatusCode;
                await context.Response.WriteAsync(response.Content);
                
                return "";
            }
            catch (Exception)
            {
                context.Response.StatusCode = 404;
                context.Response.ContentType = "text/plain";

                return "404 Not Found";
            }
        }
    }
}

