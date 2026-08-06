using System.Net;
using ragd.Cli;
using ragd.Http;

namespace ragd
{
    class Program
    {
        private const int Port = 53280;

        static int Main(string[] args)
        {
            var cmd = new Commands(
                    new LifeCycleManager(IPAddress.Loopback, Port),
                    new Client(new Parser(), IPAddress.Loopback, Port),
                    new RagLogger())
                    .Parse(args!);

            return cmd.Invoke();
        }
    }
}