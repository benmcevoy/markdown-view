namespace wikd
{
    public class Context
    {
        public int Port { get; set; } = 5001;
        public string BasePath { get; set; } = "";
        public void Log(string message) => Console.WriteLine(message);
    }
}