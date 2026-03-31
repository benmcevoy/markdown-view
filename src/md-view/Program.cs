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
            args = ["/home/agent/hello-world/sample"];
            var command = new CommandProcessor().Parse(args);

            switch (command.Name)
            {
                case CommandNames.Start: { Start(command.Parameter[0]); break; }
                default: { Help(); break; }
            }

            // TODO:
            // - command parsing is pretty shit
            // - parameters - allow render file or folder to html -r --render <target file/folder> <out?>
            // - parameters include Port as parameter -p --port 5001
            // - parameters --path <target file/folder>
            // - routing is pretty whacky - probably just want Route type with data/values like asp.net
            // - rendering is annoying as weird use of handlers, templates, etc - pick a lane
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
            var fileSystemRouter = new FileSystemRouter(rootFolder, allowedFileExtensions);
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

