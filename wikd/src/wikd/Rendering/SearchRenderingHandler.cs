using System.Net;
using wikd.Routing;

namespace wikd.Rendering;

public class SearchRenderingHandler(SearchService searchService) : IRenderingHandler
{
    private readonly SearchService _searchService = searchService;

    public string[] SupportedFileExtensions => [];

    bool IHandler<Route, string>.CanHandle(Route input) =>
            input is SpecialRoute f && input.Path == Router.SearchRoute;

    public string Handle(Route input)
    {
        var special = input as SpecialRoute;
        var q = WebUtility.UrlDecode(special!.Query["q"]);

        var query = $"Search for '{WebUtility.HtmlEncode(q)}'";
        var result = _searchService.Search(q);

        return @$"<div>
    {query}
    <hr/>
    {result}
</div>";
    }
}
