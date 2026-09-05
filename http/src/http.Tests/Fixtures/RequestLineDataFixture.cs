using System.Collections;

namespace http.Tests.Fixtures;

public class RequestLineDataFixture : IEnumerable<object[]>
{
    private readonly IList<object[]> _data =
        [
            // simple path, no query
            ["/", ("", "")],

            ["/foo", ("foo", "")],

            ["/foo/bar", ("foo/bar", "")],

            // path with query
            ["/foo?bar=baz", ("foo", "bar=baz")],

            ["/foo?bar=baz&qux=1", ("foo", "bar=baz&qux=1")],

            // root with query
            ["/?bar=baz", ("", "bar=baz")],

            // no leading slash
            ["foo", ("foo", "")],

            // trailing slash
            ["/foo/", ("foo/", "")],

            // path with fragment, no query
            ["/foo#section", ("foo", "")],

            // path with query and fragment
            ["/foo?bar=baz#section", ("foo", "bar=baz")],

            // "?" with nothing after it
            ["/foo?", ("foo", "")],

            // encoded characters left as-is at this stage (decoding happens later)
            ["/foo%20bar", ("foo%20bar", "")],

            // empty path
            ["", ("", "")],
        ];

    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var item in _data)
        {
            var path = (string)item[0];
            var (expectedPath, expectedQuery) = ((string, string))item[1];

            yield return [$"GET {path} HTTP/1.1", expectedPath, expectedQuery];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}