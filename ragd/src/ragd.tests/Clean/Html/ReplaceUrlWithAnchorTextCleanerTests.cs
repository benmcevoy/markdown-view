namespace ragd.Tests.Clean.Html;

using ragd.Clean.Html;

public class ReplaceUrlWithAnchorTextCleanerTests
{
    [Fact]
    public void ReplaceUrlWithAnchorTextCleaner_replaces_tags()
    {
        // arrange
        var sut = new ReplaceUrlWithAnchorTextCleaner();

        // act
        var actual = sut.Clean(@"this is <a href=""#"">some link</a> here

<a href='http://test.com""></a>

<a href='#'>another link</a>

<a href=""#"">test</a>");

        var expected = @"this is some link here

http://test.com

another link

test";

        // assert
        Assert.Equal(expected, actual);
    }
}