using MdView.Rendering;
using MdView.Templates;
using MdView.Routing;
using MdView.Cli;
using System.Net.Sockets;

namespace MdView
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
            if(string.IsNullOrWhiteSpace(context?.BasePath)) return;

            WriteBanner();

            Console.WriteLine($"Base path: '{context!.BasePath}'");

            var ipAddress = System.Net.IPAddress.Loopback;
            var port = context.Port;

            IRenderingHandler[] renderers = [
                new MarkdownFileRendererHandler(),
                new CodeFileRendererHandler(),
                new ImageFileRendererHandler(),
                new PdfFileRendererHandler(),
                new FolderRenderingHandler(),
                new FaviconFileRendererHandler()
                ];

            var allowedFileExtensions = renderers.SelectMany(x => x.SupportedFileExtensions).ToArray();
            var fileSystemRouter = new FileSystemRouter(context.BasePath, allowedFileExtensions);
            var renderer = new Renderer(new DefaultTemplate(), renderers);
            var router = new Router(fileSystemRouter);

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

                byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);

                stream.Write(msg, 0, msg.Length);
            }
        }

        private static void WriteBanner()
        {
            Console.WriteLine(@"               _          _               ");
            Console.WriteLine(@" _ __ ___   __| |  __   _(_) _____      __");
            Console.WriteLine(@"| '_ ` _ \ / _` |__\ \ / / |/ _ \ \ /\ / /");
            Console.WriteLine(@"| | | | | | (_| |___\ V /| |  __/\ V  V / ");
            Console.WriteLine(@"|_| |_| |_|\__,_|    \_/ |_|\___| \_/\_/  ");
            Console.WriteLine();
        }
    }
}