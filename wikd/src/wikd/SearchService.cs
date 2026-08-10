namespace wikd;

public class SearchService(string basePath)
{
    private readonly string _basePath = basePath;


    // should test for ragd somehow
    public bool IsSearchAvailable {get; private set; } = true;

    public string Search(string query)
    {
        // process.start ragd "query" --json
        return "TODO: search " + query;
    }
}