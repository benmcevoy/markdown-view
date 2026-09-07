using System.Collections;

namespace http.Tests.Fixtures;

public class QueryStringDataFixture : IEnumerable<object[]>
{
    private readonly IList<object[]> _data =
        [
            // single param
            ["?foo=bar", new Dictionary<string, string> {
                {"foo", "bar"} }],

            // multiple params
            ["?foo=bar&baz=boo", new Dictionary<string, string> {
                {"foo", "bar"}, {"baz", "boo"} }],

            // no leading "?"
            ["foo=bar", new Dictionary<string, string> ()],

            // empty query string
            ["", new Dictionary<string, string>()],

            // "?" with nothing after it
            ["?", new Dictionary<string, string>()],

            // fragment
            ["#section", new Dictionary<string, string>()],

            // query amd fragment
            ["?bar=baz#section", new Dictionary<string, string> { {"bar", "baz"} }],

            // empty segment between two ampersands should be skipped, not truncate the rest
            ["?a=1&&b=2", new Dictionary<string, string> {
                {"a", "1"}, {"b", "2"} }],

            // trailing ampersand
            ["?a=1&", new Dictionary<string, string> {
                {"a", "1"} }],

            // leading ampersand
            ["?&a=1", new Dictionary<string, string> {
                {"a", "1"} }],

            // flag-style param with no "=" should not drop subsequent params
            ["?debug&foo=bar", new Dictionary<string, string> {
                {"debug", ""}, {"foo", "bar"} }],

            // duplicate keys (e.g. multi-select) should combine, not throw
            ["?a=1&a=2", new Dictionary<string, string> {
                {"a", "1,2"} }],

            // percent-encoded key and value are decoded
            ["?na%20me=jo%20hn", new Dictionary<string, string> {
                {"na me", "jo hn"} }],

            // "+" conventionally decodes to space (application/x-www-form-urlencoded)
            ["?full+name=jo+hn", new Dictionary<string, string> {
                {"full name", "jo hn"} }],

            // value containing "=" (base64, etc.) - only first "=" splits key/value
            ["?token=abc=def", new Dictionary<string, string> {
                {"token", "abc=def"} }],

            // key with empty value
            ["?foo=", new Dictionary<string, string> {
                {"foo", ""} }],

            // query terminated by fragment
            ["?foo=bar#section", new Dictionary<string, string> {
                {"foo", "bar"} }],

            // query terminated by space (end of request line)
            ["?foo=bar ", new Dictionary<string, string> {
                {"foo", "bar"} }],

            // whitespace around key/value pairs is trimmed
            ["?foo%20=+bar+&%20baz=boo", new Dictionary<string, string> {
                {"foo", "bar"}, {"baz", "boo"} }],
        ];

    public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}