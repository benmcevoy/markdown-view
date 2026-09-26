using System.Net;
using System.Text;
using Markdig;
using wikd.Routing;

namespace wikd.Rendering;

public class SearchRenderingHandler(SearchService searchService, MarkdownPipeline pipeline, string rootPath) : IRenderingHandler
{
    private readonly SearchService _searchService = searchService;
    private readonly MarkdownPipeline _pipeline = pipeline;
    private readonly string _rootPath = rootPath;

    public string[] SupportedFileExtensions => [];

    public bool CanHandle(Route input) =>
            input is SpecialRoute f && input.Path == Router.SearchRoute;

    public async Task<string> HandleAsync(Route input)
    {
        var special = input as SpecialRoute;
        var q = WebUtility.UrlDecode(special!.Query["q"]);

        var query = $"Search for '{q}'";
        var result = await _searchService.SearchAsync(q);

        return @$"<div>
<div class='query'>{query.HtmlEncode()}</div>
<div class='results-found'>{result.Message.HtmlEncode()}</div>
<div>
    {ToSearchResultHtml(result.Data)}
</div>
</div>";
    }

    private string ToSearchResultHtml(SearchResultItem[] data)
    {
        var sb = new StringBuilder();

        foreach (var item in data)
        {
            var href = AsUri(item.SourcePath);
            sb.AppendLine(@$"<div class=""search-result-item"">");

            sb.AppendLine(@$"<a href=""{href.UrlEncode()}"">{href.HtmlEncode()}</a>");

            sb.AppendLine(@$"<div>{AsHtmlContent(item.Raw)}</div>");

            sb.AppendLine(@$"</div>");
        }

        return sb.ToString();
    }

    public string AsHtmlContent(string markdown) => Markdig.Markdown.Parse(markdown, _pipeline).ToHtml();
    private string AsUri(string path) => path.TrimStart(_rootPath).ToString();
}
