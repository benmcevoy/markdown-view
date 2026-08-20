using System.Text;
using System.Text.Encodings.Web;

namespace ragd.Http;

public static class DictionaryExtensions
{
    public static string AsQuery(this Dictionary<string, string> source)
    {
        if (source == null) return "";
        if (source.Count == 0) return "";

        var sb = new StringBuilder("?");

        foreach (var kvp in source)
        {
            sb.Append($"{UrlEncoder.Default.Encode(kvp.Key)}={UrlEncoder.Default.Encode(kvp.Value)}&");
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