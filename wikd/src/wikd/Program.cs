using wikd.Rendering;
using wikd.Templates;
using wikd.Routing;
using wikd.Cli;
using System.Net.Sockets;
using System.Reflection;

namespace wikd
{
    class Program
    {
        static void Main(string[] args)
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

            Start(context);
        }

        private static void Start(Context context)
        {
            if (string.IsNullOrWhiteSpace(context?.BasePath)) return;

            WriteBanner();

            Console.WriteLine($"Base path: '{context!.BasePath}'");

            var ipAddress = System.Net.IPAddress.Loopback;
            var port = context.Port;

            var search = new SearchService(context.BasePath);

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
            var router = new Router(new Http.Parser(), fileSystemRouter);

            using TcpListener listener = new(ipAddress, port);

            listener.Start();

            Console.WriteLine("Starting server.");
            Console.WriteLine($"Listening on: http://{ipAddress}:{port}");

            while (true)
            {
                using var client = listener.AcceptTcpClient();
                using var stream = client.GetStream();

                var route = router.Map(stream);
                var content = renderer.Render(route);
                var response = @$"HTTP/1.1 {content.StatusCode}
Content-Length: {content.Content.Length}
Content-Type: {content.ContentType}

{content.Content}";

                var msg = System.Text.Encoding.UTF8.GetBytes(response);

                stream.Write(msg, 0, msg.Length);
                stream.Flush();
            }
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




