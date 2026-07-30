using System.Text.RegularExpressions;

namespace ragd.Service.Clean.Html
{
    /// <summary>
    /// Replace Html anchors with the anchor text or url if blank
    /// </summary>
    public partial class ReplaceUrlWithAnchorTextCleaner : ICleaner
    {
        [GeneratedRegex(@"<a .*href=['|""](.*)['|""].*>(.*)<\/a>", RegexOptions.IgnoreCase)]
        private static partial Regex _tagPattern();

        public string Clean(string chunk)
        {
            foreach (Match match in _tagPattern().Matches(chunk))
            {
                var link = match.Captures[0].Value;
                var href = match.Groups[1].Value;
                var anchor = match.Groups[2].Value;

                // use the href is anchor is empty
                chunk = chunk.Replace(link, string.IsNullOrWhiteSpace(anchor) ? href : anchor);
            }

            return chunk;
        }
    }
}
