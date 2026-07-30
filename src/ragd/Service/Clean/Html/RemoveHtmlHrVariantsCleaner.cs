using System.Text.RegularExpressions;

namespace ragd.Service.Clean.Html
{
    public partial class RemoveHtmlHrVariantsCleaner : ICleaner
    {
        // my regex fu is not strong enough but this seems to work
        [GeneratedRegex(@"<hr[\s|\/|]+>|<hr>", RegexOptions.IgnoreCase)]
        private static partial Regex _tagPattern();

        public string Clean(string chunk) => _tagPattern().Replace(chunk, "");
    }
}
