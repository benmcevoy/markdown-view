namespace wikd.Cli
{
    public class PathCliCommand(string parameter) : CliCommand
    {
        private readonly string _parameter = parameter;
        private string _basePath = "";

        public override bool CanExecute() => TryResolveAbsolutePath(_parameter, out _basePath);

        public override Context Execute(Context context)
        {
            context.BasePath = _basePath!;
            return context;
        }

        public override string Error() => $"Unable to resolve path '{_parameter}'. Is it a folder?";

        private static bool TryResolveAbsolutePath(string path, out string absolutePath)
        {
            absolutePath = path;

            if (absolutePath.EndsWith(Path.DirectorySeparatorChar)) absolutePath = absolutePath.TrimEnd(Path.DirectorySeparatorChar);

            if (Directory.Exists(absolutePath))
            {
                absolutePath = Path.GetFullPath(absolutePath);
                return true;
            }

            return false;
        }
    }
}