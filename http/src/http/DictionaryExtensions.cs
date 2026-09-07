using System.Text;

namespace http;

public static class DictionaryExtensions
{
    public static string AsQueryString(this Dictionary<string, string> source)
    {
        if (source == null) return "";
        if (source.Count == 0) return "";

        var sb = new StringBuilder();

        foreach (var kvp in source)
        {
            sb.Append($"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}&");
        }

        return sb.ToString(0, sb.Length - 1);
    }

    public static string AsHeaders(this Dictionary<string, string> source)
    {
        if (source == null) return "";
        if (source.Count == 0) return "";

        var sb = new StringBuilder();

        foreach (var kvp in source)
        {
            sb.AppendLine($"{kvp.Key}: {kvp.Value}");
        }

        return sb.ToString().TrimEnd();
    }
}