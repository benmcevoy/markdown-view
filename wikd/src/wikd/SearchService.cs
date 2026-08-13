using wikd.Http;

namespace wikd;

public class SearchService
{
    private readonly string _collectionName;
    private readonly Client _client;
    private readonly bool _isInitialised = true;

    public SearchService(string collectionName, Client client)
    {
        _collectionName = collectionName;
        _client = client;
    }

    public bool IsSearchAvailable() => _isInitialised;

    public string Search(string query)
    {
        var result = _client.Send(new Request
        {
            Method = Http.HttpMethod.GET,
            Path = "query",
            Query = { {"name", _collectionName}, {"q", query}}
        });
        
        return result.Body;
    }

    private static void Index()
    {
        throw new NotImplementedException("TODO:");
    }
}

