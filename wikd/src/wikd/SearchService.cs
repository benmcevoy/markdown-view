using System.Net.Sockets;
using System.Text.Json;
using http;

namespace wikd;

public class SearchService
{
    private readonly string _collectionName;
    private readonly Client _client;
    private readonly bool _isInitialised = true;

    public SearchService(string rootPath, Client client)
    {
        _collectionName = rootPath.AsCollectionName();
        _client = client;
    }

    public bool IsSearchAvailable() => _isInitialised;

    public async Task<SearchResult> SearchAsync(string query)
    {
        try
        {
            var response = await _client.SendAsync(new Request
            {
                Method = http.HttpMethod.GET,
                Path = "query",
                Query = { { "name", _collectionName }, { "q", query } }
            });

            var results = await JsonSerializer.DeserializeAsync(response.Body, JsonSerializerJsonContext.Default.SearchResult);

            return results ?? new SearchResult { Message = "No results found", Status = "No results" };
        }
        catch (SocketException ex)
        {
            return new SearchResult { Message = ex.Message, Status = "Error" };
        }
    }

    private static void Index()
    {
        throw new NotImplementedException("TODO:");
    }
}


public class SearchResult
{
    /*

{
    "message": "Found 1 results.",
    "status": "OK",
    "data": [
        {
            "raw": "## Value\nPossible values are:\n- \u0060none\u0060 or \u0060off\u0060\n- : Do not automatically capitalize any text.\n- \u0060sentences\u0060 or \u0060on\u0060\n- : Automatically capitalize the first character of each sentence.\n- \u0060words\u0060\n- : Automatically capitalize the first character of each word.\n- \u0060characters\u0060\n- : Automatically capitalize every character.",
            "content": "Value\nPossible values are:\nnone or off\n: Do not automatically capitalize any text.\nsentences or on\n: Automatically capitalize the first character of each sentence.\nwords\n: Automatically capitalize the first character of each word.\ncharacters\n: Automatically capitalize every character.",
            "score": 0.80992126,
            "name": "media-ben-data-dev-git-agents-rag-docs-test-mdn-html",
            "sourcePath": "/media/ben/DATA/Dev/git/agents/RAG/docs/test/mdn-html/reference/global_attributes/autocapitalize/index.md",
            "chunkPath": "Value",
            "chunkIndex": 2,
            "totalChunks": 10,
            "startOffset": 1030,
            "endOffset": 1357,
            "createdAt": "2026-09-24T06:29:53.2145042Z"
        }
    ]
}

    */

    public string Message { get; set; } = "";
    public string Status { get; set; } = "";
    public SearchResultItem[] Data { get; set; } = [];
}

public class SearchResultItem
{
    public string Raw { get; set; } = "";
    public string Content { get; set; } = "";
    public string SourcePath { get; set; } = "";
    public float Score { get; set; }
    public int StartOffset { get; set; }
    public int EndOffset { get; set; }
}
