using System.Collections.ObjectModel;
using System.Net;

namespace ragd;

public class Config
{
    public string DatabasePath { get; set; } = "./ragd.db";
    public string ModelPath { get; set; } = "./ragd.gguf";
    public IPAddress Host { get; set; } = IPAddress.Loopback;
    public int Port { get; set; } = 53280;
    public string VectorExtensionPath { get; set; } = "./vec0.so";
    public int TopNResults { get; set; } = 3;

    public override string ToString() => @$"
config.DatabasePath: {new FileInfo(DatabasePath).FullName}
config.ModelPath: {new FileInfo(ModelPath).FullName}
config.Host: {Host}
config.Port: {Port}
config.TopNResults: {TopNResults}    
config.VectorExtensionPath: {new FileInfo(VectorExtensionPath).FullName}
";
    public (bool, Collection<string>) Validate()
    {
        var results = new Collection<string>();
        var isValid = true;

        var test = new FileInfo(VectorExtensionPath);
        if (!test.Exists)
        {
            isValid = false;
            results.Add($"Unable to find configured {nameof(VectorExtensionPath)} at '{test.FullName}'");
        }

        test = new FileInfo(ModelPath);
        if (!test.Exists)
        {
            isValid = false;
            results.Add($"Unable to find configured {nameof(ModelPath)} at '{test.FullName}'");
        }

        test = new FileInfo(DatabasePath);
        if (!test.Exists)
        {
            isValid = false;
            results.Add(@$"Unable to find configured {nameof(DatabasePath)} at '{test.FullName}'.
To create new database use:
ragd new ""{test.FullName}""");
        }

        if (Port < 1 || Port > 65535)
        {
            isValid = false;
            results.Add($"{nameof(Port)} must be between 0 and 65536");
        }

        if (TopNResults < 0)
        {
            isValid = false;
            results.Add($"{nameof(TopNResults)} must be greater than 0");
        }

        return (isValid, results);
    }
}