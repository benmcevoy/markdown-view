
using System.Net;
using System.Text.RegularExpressions;

namespace wikd;
public static partial class StringExtensions
{
    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();

    public static string AsCollectionName(this string path)
        => NonAlphanumericRegex().Replace(path.ToLowerInvariant(), "-").Trim('-');

    public static string HtmlEncode(this string value) => WebUtility.HtmlEncode(value);
    public static string HtmlDecode(this string value) => WebUtility.HtmlDecode(value);
    public static string UrlEncode(this string value) => WebUtility.UrlEncode(value);
    public static string UrlDecode(this string value) => WebUtility.UrlDecode(value);
}