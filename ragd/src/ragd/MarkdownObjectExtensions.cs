using Markdig.Renderers.Normalize;
using Markdig.Syntax;

namespace ragd;

public static class MarkdownObjectExtensions
{
    public static string ToMarkdown(this MarkdownObject mdo) => ToMarkdown(mdo, new NormalizeOptions());

    public static string ToMarkdown(this MarkdownObject mdo, NormalizeOptions normalizeOptions)
    {
        using var writer = new StringWriter();
        var renderer = new NormalizeRenderer(writer, normalizeOptions);

        return renderer.Render(mdo).ToString() ?? "";
    }

    public static bool TryGetBlockAs<T>(this MarkdownObject mdo, out T? block) where T : Block
    {
        block = mdo as T;
        return block != null;
    }
}