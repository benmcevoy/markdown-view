namespace wikd.Cli
{
    public abstract class CliCommand
    {
        public abstract bool CanExecute();
        public abstract Context Execute(Context context);
        public abstract string Error();
    }
}