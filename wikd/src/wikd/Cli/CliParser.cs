namespace wikd.Cli
{
    public class CliParser
    {
        public static ICollection<CliCommand> Parse(string[] args)
        {
            if (args == null || args.Length == 0) return [new HelpCliCommand()];

            var result = new List<CliCommand>();

            for (var i = 0; i < args.Length; i++)
            {
                var current = args[i];

                if (IsHelp(current)) return [new HelpCliCommand()];

                if (IsPort(current))
                {
                    result.Add(new PortCliCommand(args[++i]));
                    continue;
                }

                result.Add(new PathCliCommand(current));
            }

            return result;
        }

        private static bool IsHelp(string arg) => arg.Trim() == "-h" || arg.Trim() == "-?" || arg.Trim() == "--help";

        private static bool IsPort(string arg) => arg.Trim() == "-p" || arg.Trim() == "--port";
    }
}