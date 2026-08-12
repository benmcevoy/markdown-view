using System.Collections;

namespace ragd.Tests.Http;

public class QueryStringDataFixture : IEnumerable<object[]>
{
    private readonly IList<object[]> _data =
        [
            ["?a=b&c=d", new Dictionary<string, string> { {"a","b"}, {"c","d"}}],
            ["?a=&c=d", new Dictionary<string, string> { {"a",""}, {"c","d"}}],
            // TODO: issues - asp.net core uses <string, stringvalues> which sometimes evaluates to comma separated
            // alternatively use <string, string[]>
            //["?a=&c=d&&a=e&a=f&a=&a=g,h&", new Dictionary<string, string> { {"a","e,f,g,h"}, {"c","d"}}],
            //["?a= & c = d& &", new Dictionary<string, string> { {"a","e,f,g,h"}, {"c","d"}}],
        ];

    public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}