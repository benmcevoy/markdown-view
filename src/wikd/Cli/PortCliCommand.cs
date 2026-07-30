namespace wikd.Cli
{
    public class PortCliCommand(string parameter) : CliCommand
    {
        private readonly string _parameter = parameter;
        private int _port;

        public override bool CanExecute() => int.TryParse(_parameter, out _port);

        public override string Error() => $"Unable to resolve port '{_parameter}'.";

        public override Context Execute(Context context)
        {
            context.Port = _port;
            return context;
        }
    }
}