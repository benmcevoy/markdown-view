using System.Collections;

namespace http.Tests.Fixtures;

// Covers parsing of the request-side "Cookie" header (semicolon-separated
// name=value pairs) into a Dictionary<string, string>.
//
// NOTE: "Set-Cookie" (response side) is deliberately NOT covered here -
// it needs List<string> semantics (multiple headers, one full attribute
// string each, values may contain commas) rather than a single merged
// dictionary. See conversation notes before reusing this shape for it.
public class CookieDataFixture : IEnumerable<object[]>
{
    private readonly IList<object[]> _data =
        [
            // single cookie
            ["name1=value1", new Dictionary<string, string> {
                {"name1", "value1"} }],

            // multiple cookies
            ["name1=value1; name2=value2", new Dictionary<string, string> {
                {"name1", "value1"}, {"name2", "value2"} }],

            // no space after semicolon
            ["name1=value1;name2=value2", new Dictionary<string, string> {
                {"name1", "value1"}, {"name2", "value2"} }],

            // empty cookie header
            ["", new Dictionary<string, string>()],

            // trailing semicolon
            ["name1=value1;", new Dictionary<string, string> {
                {"name1", "value1"} }],

            // leading semicolon
            ["; name1=value1", new Dictionary<string, string> {
                {"name1", "value1"} }],

            // empty segment between semicolons should be skipped, not truncate the rest
            ["name1=value1;; name2=value2", new Dictionary<string, string> {
                {"name1", "value1"}, {"name2", "value2"} }],

            // value containing "=" (base64, etc.) - only first "=" splits name/value
            ["token=abc=def", new Dictionary<string, string> {
                {"token", "abc=def"} }],

            // cookie name is case-sensitive per RFC 6265 (unlike header names)
            [@"Name1=value1; name1=value2", new Dictionary<string, string> {
                {"Name1", "value1"}, {"name1", "value2"} }],

            // values are NOT URL-decoded per spec (unlike query string values)
            ["name1=va%20lue1", new Dictionary<string, string> {
                {"name1", "va%20lue1"} }],

            // quoted cookie value (RFC 6265 permits DQUOTE-wrapped values)
            [@"name1=""value1""", new Dictionary<string, string> {
                {"name1", "\"value1\""} }],

            // flag-style entry with no "=" should not drop subsequent pairs
            ["flag; name1=value1", new Dictionary<string, string> {
                {"flag", ""}, {"name1", "value1"} }],
        ];

    public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}