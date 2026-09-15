using System.CommandLine;

namespace ragd.Cli.Options;

public class DatabaseOption : Option<FileInfo>
{
    public const string OptionName = "--database";

    public DatabaseOption(bool databaseMustExistIsOptional = false, bool optional = false) : base(OptionName, "-db")
    {
        Description = "Path to sqlite database, e.g. './rag.db'";
        Required = !optional;
        AcceptLegalFilePathsOnly();
        if (!databaseMustExistIsOptional) Validators.Add(OptionResultValidators.FileExists);
    }
}
