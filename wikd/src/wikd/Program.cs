using wikd.Rendering;
using wikd.Templates;
using wikd.Routing;
using wikd.Cli;

namespace wikd
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // TODO:
            // - routing is pretty whacky - probably just want Route type with data/values like asp.net
            // - routing is... not even routing. should map to an action, might be better to have "requestHandler"
            // - "special" routes are stupid. route inheritance is stupid.
            // - rendering is annoying as weird use of handlers, templates, etc - pick a lane
            // - my tests suck

            var context = new Context();
            var commands = CliParser.Parse(args);

            foreach (var command in commands)
            {
                if (command.CanExecute())
                {
                    context = command.Execute(context);
                    continue;
                }

                var color = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(command.Error());
                Console.ForegroundColor = color;

                return;
            }

            await Start(context);
        }

        private static async Task Start(Context context)
        {
            if (string.IsNullOrWhiteSpace(context?.BasePath)) return;

            WriteBanner();

            Console.WriteLine($"Base path: '{context!.BasePath}'");

            var ipAddress = System.Net.IPAddress.Loopback;
            var port = context.Port;
            var search = new SearchService(context.BasePath, new http.Client(ipAddress, 53280));

            IRenderingHandler[] renderers = [
                new MarkdownFileRenderingHandler(),
                new CodeFileRenderingHandler(),
                new ImageFileRenderingHandler(),
                new PdfFileRenderingHandler(),
                new FolderRenderingHandler(),
                new SearchRenderingHandler(search),
                new AdminRenderingHandler(),
                new DefaultSpecialRenderingHandler()
                ];

            var allowedFileExtensions = renderers.SelectMany(x => x.SupportedFileExtensions).ToArray();
            var fileSystemRouter = new FileSystemRouter(context.BasePath, allowedFileExtensions);
            var renderer = new Renderer(new DefaultTemplate(), renderers);
            var router = new Router(fileSystemRouter, renderer);

            var daemon = new http.Daemon(ipAddress, port)
            {
                RequestHandler = router.RequestReceived
            };

            Console.WriteLine("Starting server.");
            Console.WriteLine($"Listening on: http://{ipAddress}:{port}");
 
            await daemon.Start(CancellationToken.None);
        }

        private static void WriteBanner()
        {
            // figlet font is "DOS-Rebel"  apparantly, cool
            Console.WriteLine();
            var c = Console.ForegroundColor;
            Console.ForegroundColor = (ConsoleColor)Random.Shared.Next(1, 16);
            Console.WriteLine(@"                  ███  █████          █████");
            Console.WriteLine(@"                 ░░░  ░░███          ░░███ ");
            Console.WriteLine(@" █████ ███ █████ ████  ░███ █████  ███████ ");
            Console.WriteLine(@"░░███ ░███░░███ ░░███  ░███░░███  ███░░███ ");
            Console.WriteLine(@" ░███ ░███ ░███  ░███  ░██████░  ░███ ░███ ");
            Console.WriteLine(@" ░░███████████   ░███  ░███░░███ ░███ ░███ ");
            Console.WriteLine(@"  ░░████░████    █████ ████ █████░░████████");
            Console.WriteLine(@"   ░░░░ ░░░░    ░░░░░ ░░░░ ░░░░░  ░░░░░░░░ ");
            Console.ForegroundColor = c;
            Console.WriteLine();
        }
    }
}




