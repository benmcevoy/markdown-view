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
            //args = ["new", "-db", "./app.db", "-m", "/media/ben/DATA/Dev/git/agents/models/Qwen3-Embedding-0.6B-Q8_0.gguf"];
            //args = ["start", "-db", "./app.db", "-m", "/media/ben/DATA/Dev/git/agents/models/Qwen3-Embedding-0.6B-Q8_0.gguf"];
            args = ["listen", "-db", "./app.db", "-m", "/media/ben/DATA/Dev/git/agents/models/Qwen3-Embedding-0.6B-Q8_0.gguf"];
            //args = ["status"];
            //args = ["query", "use a hit box"];
            //args = ["stop", "-q", "--json"];
            //args = ["index", "/media/ben/DATA/Dev/git/agents/RAG/docs/test/mdn-html"];

            var cmd = new Commands(
                    new LifeCycleManager(IPAddress.Loopback, Port),
                    new Client(new Parser(), IPAddress.Loopback, Port),
                    new RagLogger())
                    .Parse(args!);

            return cmd.Invoke();
        }
    }
}