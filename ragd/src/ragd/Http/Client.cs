using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ragd.Http;

public class Client(Parser parser, IPAddress host, int port)
{
    private readonly Parser _parser = parser;
    private readonly IPAddress _host = host;
    private readonly int _port = port;

    public JsonResponse Send(Request request)
    {
        using var client = new TcpClient(_host.ToString(), _port);

        var data = Encoding.UTF8.GetBytes(request.ToString());
        using var stream = client.GetStream();

        stream.Write(data, 0, data.Length);

        return _parser.ParseResponse(stream);
    }
}