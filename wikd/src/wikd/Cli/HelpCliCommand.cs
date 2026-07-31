namespace wikd.Cli
{
  public class HelpCliCommand : CliCommand
  {
    private const string Help = @"
Description:
  Lightweight readonly wiki web server of markdown and knowledge base files.

Usage: 
  wikd [path] [options]                 e.g. ./wikd .

path:
  The path to a folder to serve as a markdown viewer site.

Options:
  -?, -h, --help                         Show help and usage information
  -p <port>, --port <port>               Specify listen port (Default: 5001), e.g. http://localhost:<port>
";

    public override bool CanExecute() => true;

    public override string Error() => "";

    public override Context Execute(Context context)
    {
      // reset context
      context = new Context();
      context.Log(Help);
      return context;
    }
  }
}