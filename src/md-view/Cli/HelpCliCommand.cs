namespace MdView.Cli
{
  public class HelpCliCommand : CliCommand
  {
    private const string Help = @"
Usage: md-view [path-to-folder] [commands]

path-to-folder:
  The path to a folder to serve as a markdown viewer site.

commands:
  -h|--help                         Display help.
  -p|--port <port>                  Specify listen port (Default: 5001), e.g. http://localhost:<port>
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