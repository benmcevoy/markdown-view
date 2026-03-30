namespace MdView.Cli
{
    public class CliCommand
    {
        public CommandNames Name { get; set; } = CommandNames.Help;
        public string[] Parameter { get; set; } = [];
        public static CliCommand Help => new();
    }
}