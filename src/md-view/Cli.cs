namespace MdView
{
    public class Cli
    {
        public CliCommand Parse(string[] args)
        {
            if (args == null) return CliCommand.Help;
            if (args.Length == 0) return CliCommand.Help;

            var path = args[0];

            if (IsHelp(path)) return CliCommand.Help;

            if (path.EndsWith(Path.DirectorySeparatorChar)) path = path.TrimEnd(Path.DirectorySeparatorChar);

            if (Directory.Exists(path)) return new CliCommand { Name = CommandNames.Start, Parameter = path };

            return new CliCommand { Name = CommandNames.Help, Parameter = path };
        }

        private static bool IsHelp(string arg) => (arg.Trim() == "-h" || arg.Trim() == "--help");

        public enum CommandNames { Help, Start }
    }

    public class CliCommand
    {
        public Cli.CommandNames Name { get; set; } = Cli.CommandNames.Help;
        public string Parameter { get; set; } = "";
        public static CliCommand Help => new();
    }
}