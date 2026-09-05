using System.Collections;

namespace http.Tests.Fixtures;

public class HeadersDataFixture : IEnumerable<object[]>
{
    private readonly IList<object[]> _data =
        [
            // single header, standard "Key: value" spacing
            ["Content-Type: plain/text", new Dictionary<string, string> {
                {"Content-Type", "plain/text"} }],

            // multiple headers
            [@"Content-Type: plain/text
Content-Length: 123", new Dictionary<string, string> {
                {"Content-Type", "plain/text"}, {"Content-Length", "123"} }],

            // header key lookup is case-insensitive
            [@"content-type: plain/text", new Dictionary<string, string> {
                {"Content-Type", "plain/text"} }],

            // no space after colon
            ["Content-Type:plain/text", new Dictionary<string, string> {
                {"Content-Type", "plain/text"} }],

            // multiple spaces after colon
            ["Content-Type:   plain/text", new Dictionary<string, string> {
                {"Content-Type", "plain/text"} }],

            // empty value
            ["X-Empty:", new Dictionary<string, string> {
                {"X-Empty", ""} }],

            // value containing a colon (URLs, times, etc.)
            ["Location: http://example.com:8080/path", new Dictionary<string, string> {
                {"Location", "http://example.com:8080/path"} }],

            // duplicate header names (legal per RFC 7230, should combine not throw)
            [@"X-Forwarded-For: 10.0.0.1
X-Forwarded-For: 10.0.0.2", new Dictionary<string, string> {
                {"X-Forwarded-For", "10.0.0.1,10.0.0.2"} }],

            // header name with allowed special token characters
            ["X-Custom-Header_1: value", new Dictionary<string, string> {
                {"X-Custom-Header_1", "value"} }],

            // trailing whitespace in value
            ["Content-Type: plain/text   ", new Dictionary<string, string> {
                {"Content-Type", "plain/text"} }],

            // blank line terminates header parsing (nothing after it should be included)
            [@"Content-Type: plain/text

X-Should-Not-Appear: value", new Dictionary<string, string> {
                {"Content-Type", "plain/text"} }],

            // no headers at all
            ["", new Dictionary<string, string>()],
        ];

    public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}