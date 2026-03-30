namespace MdView.Cli
{
    public class CommandProcessor
    {
        public CliCommand Parse(string[] args)
        {
            if (args == null) return CliCommand.Help;
            if (args.Length == 0) return CliCommand.Help;
            if (IsHelp(args[0])) return CliCommand.Help;

            if (IsRender(args[0]))
            {
                return new CliCommand { Name = CommandNames.Render, Parameter = ["TODO: input file/folder", "TODO: out folder"] };
            }

            var path = ResolveAbsolutePath(args[0]);

            return new CliCommand { Name = CommandNames.Start, Parameter = [path] };
        }

        private static string ResolveAbsolutePath(string path)
        {
            if (path.EndsWith(Path.DirectorySeparatorChar)) path = path.TrimEnd(Path.DirectorySeparatorChar);
            if (Directory.Exists(path)) return Path.GetFullPath(path);

            throw new NotSupportedException($"cannot resolve path : '{path}'");
        }

        private static bool IsHelp(string arg) => (arg.Trim() == "-h" || arg.Trim() == "--help");
        private static bool IsRender(string arg) => (arg.Trim() == "-r" || arg.Trim() == "--render");
    }
}