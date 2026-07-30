using System.CommandLine;

namespace ragd.Cli.Options;

public class DatabaseOption : Option<FileInfo>
{
    public const string OptionName = "--database";

    public DatabaseOption(bool databaseMustExistIsOptional = false) : base(OptionName, "-db")
    {
        Description = "Path to sqlite database, e.g. './rag.db'";
        Required = true;
        AcceptLegalFilePathsOnly();
        if (!databaseMustExistIsOptional) Validators.Add(OptionResultValidators.FileExists);
    }
}
