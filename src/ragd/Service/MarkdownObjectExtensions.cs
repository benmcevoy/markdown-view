using Markdig.Renderers.Normalize;
using Markdig.Syntax;

namespace ragd.Service;

public static class MarkdownObjectExtensions
{
    public static string ToMarkdown(this MarkdownObject mdo) => ToMarkdown(mdo, new NormalizeOptions());

    public static string ToMarkdown(this MarkdownObject mdo, NormalizeOptions normalizeOptions)
    {
        using var writer = new StringWriter();
        var renderer = new NormalizeRenderer(writer, normalizeOptions);

        return renderer.Render(mdo).ToString() ?? "";
    }

    public static T? TryGetBlock<T>(this MarkdownObject mdo, out bool exists) where T : Block
    {
        var candidate = mdo as T;
        exists = candidate != null;
        return candidate;
    }
}