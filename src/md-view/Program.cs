using MdView.Rendering;
using MdView.Templates;
using MdView.FileSystem;
using MdView.Cli;
using System.Net.Sockets;

namespace MdView
{
    class Program
    {
        static void Main(string[] args)
        {
            args = ["/home/agent/hello-world/sample"];
            var command = new CommandProcessor().Parse(args);

            switch (command.Name)
            {
                case CommandNames.Start: { Start(command.Parameter[0]); break; }
                default: { Help(); break; }
            }

            // TODO:
            // - Error handling, 404 page
            // - parameters include Port as parameter
            // 
        }

        private static void Start(string rootFolder)
        {
            WriteBanner();
            Console.WriteLine($"Base path: '{rootFolder}'");

            var ipAddress = System.Net.IPAddress.Loopback;
            var port = 5001;

            IRenderingHandler[] renderers = [
                new MarkdownFileRendererHandler(),
                new CodeFileRendererHandler(),
                new ImageFileRendererHandler(),
                new PdfFileRendererHandler(),
                new FolderRenderingHandler(),
                new FaviconFileRendererHandler()
                ];

            var allowedFileExtensions = renderers.SelectMany(x => x.SupportedFileExtensions).ToArray();
            var fileSystemInfoService = new FileSystemInfoService(rootFolder, allowedFileExtensions);
            var navigation = new Navigation(fileSystemInfoService);
            var renderer = new Renderer(new DefaultTemplate(), navigation, renderers);
            var router = new Router(fileSystemInfoService);

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
                var response = $"HTTP/1.1 200 OK\r\nContent-Length: {content.Length}\r\nContent-Type: text/html\r\n\r\n{content}";

                byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);

                stream.Write(msg, 0, msg.Length);
            }
        }

        private static void Help()
        {
            const string help = @"
Usage: md-view [path-to-folder]

path-to-folder:
  The path to a folder to serve as a markdown viewer site.

commands:
  -h|--help                         Display help.
";

            Console.Write(help);
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

