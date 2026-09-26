using System.CommandLine;
using System.Text;
using System.Text.Json;
using ragd.Cli.Options;

namespace ragd.Cli;

public class ListenCommand : Command
{
    private readonly LifeCycleManager _lifeCycleManager;

    public ListenCommand(LifeCycleManager lifeCycleManager) : base("listen")
    {
        _lifeCycleManager = lifeCycleManager;

        Hidden = true;

        Options.Add(new ConfigOption());

        SetAction(Listen);
    }

    private void Listen(ParseResult parseResult)
    {
        var config64 = parseResult.GetRequiredValue<string>(ConfigOption.OptionName);
        var config = JsonSerializer.Deserialize<Config>(Convert.FromBase64String(config64));

        if(config is null) throw new Exception("Configuration invalid - Unable to start RAG daemon.");

        var (isValid, errors) = config.Validate();
        if (!isValid)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
            Console.ForegroundColor = c;

            throw new Exception("Configuration invalid - Unable to start RAG daemon.");
        }

        _lifeCycleManager.Listen(config!);
    }
}
